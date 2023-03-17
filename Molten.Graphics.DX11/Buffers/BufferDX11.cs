using System.Runtime.CompilerServices;
using Molten.Collections;
using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public abstract unsafe class BufferDX11 : GraphicsResourceDX11<ID3D11Buffer>, IGraphicsBuffer
    {
        ID3D11Buffer* _native;
        uint _ringPos;

        internal BufferDesc Desc;
        ThreadedQueue<IBufferOperation> _pendingChanges;

        internal BufferDX11(DeviceDX11 device,
            BufferFlags bufferFlags,
            BindFlag bindFlags,
            uint stride,
            uint numElements,
            ResourceMiscFlag optionFlags = 0,
            void* initialData = null) : base(device,
                ((bindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess ? GraphicsBindTypeFlags.Output : GraphicsBindTypeFlags.None) |
                ((bindFlags & BindFlag.ShaderResource) == BindFlag.ShaderResource ? GraphicsBindTypeFlags.Input : GraphicsBindTypeFlags.None))
        {
            Flags = bufferFlags;
            Stride = stride;
            ByteCapacity = Stride * numElements;
            ElementCount = numElements;
            _pendingChanges = new ThreadedQueue<IBufferOperation>();

            InitializeBuffer( bindFlags, optionFlags, initialData);
            device.ProcessDebugLayerMessages();
        }

        internal BufferDX11(DeviceDX11 device,
            BufferFlags bufferFlags,
            BindFlag bindFlags,
            uint numBytes,
            ResourceMiscFlag optionFlags = 0,
            void* initialData = null) : base(device,
        ((bindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess ? GraphicsBindTypeFlags.Output : GraphicsBindTypeFlags.None) |
        ((bindFlags & BindFlag.ShaderResource) == BindFlag.ShaderResource ? GraphicsBindTypeFlags.Input : GraphicsBindTypeFlags.None))
        {
            Flags = bufferFlags;
            Stride = 0;
            ByteCapacity = numBytes;
            ElementCount = 0;
            _pendingChanges = new ThreadedQueue<IBufferOperation>();

            InitializeBuffer( bindFlags, optionFlags, initialData);
            device.ProcessDebugLayerMessages();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void QueueOperation(GraphicsPriority priority, IBufferOperation op)
        {
            if (priority == GraphicsPriority.Immediate)
                op.Process(Device.Cmd);
            else
                _pendingChanges.Enqueue(op);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialDataPtr">A pointer to data that the buffer should initially be populated with.</param>
        protected virtual void InitializeBuffer(BindFlag bindFlags, ResourceMiscFlag opFlags, void* initialData)
        {
            DeviceDX11 nDevice = Device as DeviceDX11;
            if (Flags.IsImmutable() && initialData == null)
                throw new GraphicsBufferException(this, "Initial data cannot be null when buffer mode is Immutable.");

            Desc = new BufferDesc();
            Desc.ByteWidth = ByteCapacity;
            Desc.StructureByteStride = 0;

            // Only staging allows CPU reads.
            // See for ref: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_usage
            if (Flags.HasFlags(BufferFlags.CpuRead))
            {
                Desc.Usage = Usage.Staging;
                Desc.MiscFlags = 0U;
                Desc.BindFlags = 0U;
                Desc.CPUAccessFlags |= (uint)CpuAccessFlag.Read;

                if (Flags.HasFlags(BufferFlags.CpuWrite))
                    Desc.CPUAccessFlags |= (uint)CpuAccessFlag.Write;
            }
            else
            {
                Desc.Usage = Usage.Default;
                Desc.BindFlags = (uint)bindFlags;
                Desc.MiscFlags = (uint)opFlags;
                Desc.CPUAccessFlags = (uint)CpuAccessFlag.None;

                // Only dynamic buffers allow CPU-write, excluding a staging buffer.
                if (Flags.HasFlags(BufferFlags.CpuWrite))
                {
                    Desc.Usage = Usage.Dynamic;
                    Desc.CPUAccessFlags = (uint)CpuAccessFlag.Write;
                }
                else if(Flags.HasFlags(BufferFlags.GpuRead))
                {
                    // GPU read-only?
                    if(!Flags.HasFlags(BufferFlags.GpuWrite))
                        Desc.Usage = Usage.Immutable;  
                }
                else
                {
                    throw new GraphicsBufferException(this, "Invalid BufferFlags value");
                }
            }

            // Ensure structured buffers get the stride info.
            if (Desc.MiscFlags == (uint)ResourceMiscFlag.BufferStructured)
                Desc.StructureByteStride = Stride;

            if (initialData != null)
            {
                SubresourceData srd = new SubresourceData(initialData, ByteCapacity, ByteCapacity);
                nDevice.Ptr->CreateBuffer(ref Desc, ref srd, ref _native);
            }
            else
            {
                nDevice.Ptr->CreateBuffer(ref Desc, null, ref _native);
            }

            Device.AllocateVRAM(ByteCapacity);
            CreateResources();
        }

        protected virtual void CreateResources()
        {
            if (HasBindFlags(BindFlag.ShaderResource))
            {
                SRV.Desc = new ShaderResourceViewDesc1()
                {
                    Buffer = new BufferSrv()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                SRV.Create(this);
            }

            if (HasBindFlags(BindFlag.UnorderedAccess))
            {
                UAV.Desc = new UnorderedAccessViewDesc1()
                {
                    Format = Format.FormatUnknown,
                    ViewDimension = UavDimension.Buffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = 0, // TODO add support for append, raw and counter buffers. See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_buffer_uav_flag
                    }
                };
                UAV.Create(this);
            }
        }

        /// <summary>Copies all the data in the current <see cref="BufferDX11"/> to the destination <see cref="BufferDX11"/>.</summary>
        /// <param name="cmd">The <see cref="CommandQueueDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="BufferDX11"/> to copy to.</param>
        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, Action completionCallback = null)
        {
            if (ByteCapacity < Desc.ByteWidth)
                throw new GraphicsBufferException(this, "The destination buffer is not large enough.");

            QueueOperation(priority, new BufferDirectCopyOperation()
            {
                SrcBuffer = this,
                DestBuffer = destination as BufferDX11,
                CompletionCallback = completionCallback,
            });
        }

        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, Action completionCallback = null)
        {
            QueueOperation(priority, new BufferCopyOperation()
            {
                CompletionCallback = completionCallback,
                SrcBuffer = this,
                DestBuffer = destination as BufferDX11,
                DestByteOffset = destByteOffset,
                SrcRegion = sourceRegion.ToApi(),
            });
        }

        public void GetStream(GraphicsPriority priority, Action<IGraphicsBuffer, RawStream> callback, IStagingBuffer staging = null)
        {
            QueueOperation(priority, new BufferGetStreamOperation()
            {
                ByteOffset = 0,
                NumElements = ElementCount,
                SrcBuffer = this,
                Staging = staging,
                StreamCallback = callback,
                Stride = Stride
            });
        }

        internal void GetStream(CommandQueueDX11 cmd,
            uint byteOffset,
            uint stride,
            uint elementCount,
            Action<BufferDX11, RawStream> callback,
            StagingBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Desc.Usage == Usage.Dynamic;
            bool isStaged = Desc.Usage == Usage.Staging &&
                (Desc.CPUAccessFlags & (uint)CpuAccessFlag.Write) == (uint)CpuAccessFlag.Write;

            uint numBytes = stride * elementCount;
            RawStream stream;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                if (Flags.HasFlags(BufferFlags.Discard))
                {
                    cmd.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                    stream.Position = byteOffset;
                    cmd.Profiler.Current.MapDiscardCount++;
                }
                else if (Flags.HasFlags(BufferFlags.Ring))
                {
                    // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                    // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                    if (HasBindFlags(BindFlag.VertexBuffer) || HasBindFlags(BindFlag.IndexBuffer))
                    {
                        if (_ringPos > 0 && _ringPos + numBytes < Desc.ByteWidth)
                        {
                            cmd.MapResource(NativePtr, 0, Map.WriteNoOverwrite, 0, out stream);
                            cmd.Profiler.Current.MapNoOverwriteCount++;
                            stream.Position = _ringPos;
                            _ringPos += numBytes;
                        }
                        else
                        {
                            cmd.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                            cmd.Profiler.Current.MapDiscardCount++;
                            stream.Position = 0;
                            _ringPos = numBytes;
                        }
                    }
                    else
                    {
                        cmd.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                        cmd.Profiler.Current.MapDiscardCount++;
                        stream.Position = byteOffset;
                    }
                }
                else
                {
                    cmd.MapResource(NativePtr, 0, Map.Write, 0, out stream);
                    cmd.Profiler.Current.MapWriteCount++;
                }

                callback(this, stream);
                cmd.UnmapResource(NativePtr, 0);
            }
            else
            {
                // Write to the provided staging buffer instead.
                if (staging == null)
                    throw new GraphicsBufferException(this, "Staging buffer required. Non-dynamic/staged buffers require a staging buffer to access data.");

                isDynamic = staging.Desc.Usage == Usage.Dynamic;
                isStaged = staging.Desc.Usage == Usage.Staging;

                if (!isDynamic && !isStaged)
                    throw new GraphicsBufferException(this, "The provided staging buffer is invalid. Must be either dynamic or staged.");

                if (staging.Desc.ByteWidth < numBytes)
                    throw new GraphicsBufferException(this, $"The provided staging buffer is not large enough ({staging.Desc.ByteWidth} bytes) to fit the provided data ({numBytes} bytes).");

                // Write updated data into buffer
                if (isDynamic) // Always discard staging buffer data, since the old data is no longer needed after it's been copied to it's target resource.
                {
                    cmd.MapResource(staging.NativePtr, 0, Map.WriteDiscard, 0, out stream);
                    cmd.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    cmd.MapResource(staging.NativePtr, 0, Map.Write, 0, out stream);
                    cmd.Profiler.Current.MapWriteCount++;
                }

                callback(staging, stream);
                cmd.UnmapResource(staging.NativePtr, 0);

                Box stagingRegion = new Box()
                {
                    Left = 0,
                    Right = numBytes,
                    Back = 1,
                    Bottom = 1,
                };
                cmd.CopyResourceRegion(staging, 0, ref stagingRegion,
                    this, 0, new Vector3UI(byteOffset, 0, 0));
                cmd.Profiler.Current.CopySubresourceCount++;
            }
        }

        public void SetData<T>(GraphicsPriority priority, T[] data, IStagingBuffer staging = null, Action completeCallback = null)
            where T : unmanaged
        {
            SetData(priority, data, 0, (uint)data.Length, 0, staging, completeCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="data"></param>
        /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
        /// <param name="elementCount"></param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        /// <param name="staging"></param>
        /// <param name="completeCallback"></param>
        public void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, uint byteOffset = 0, 
            IStagingBuffer staging = null, Action completeCallback = null)
            where T : unmanaged
        {
            BufferSetOperation<T> op = new BufferSetOperation<T>()
            {
                ByteOffset = byteOffset,
                CompletionCallback = completeCallback,
                DestBuffer = this,
                ElementCount = elementCount,
                Stride = (uint)sizeof(T),
                Staging = staging as StagingBuffer,
            };

            // Custom handling of immediate command, so that we potentially avoid a data copy.
            if(priority == GraphicsPriority.Immediate)
            {
                op.Data = data;
                op.DataStartIndex = startIndex;
                op.Process(Device.Cmd);
            }
            else
            {
                // Only copy the part we need from the source data, starting from startIndex.
                op.Data = new T[data.Length];
                op.DataStartIndex = 0;
                Array.Copy(data, (int)startIndex, op.Data, 0, elementCount);
                QueueOperation(priority, op);
            }
        }

        /// <summary>Retrieves data from a <see cref="BufferDX11"/>.</summary>
        /// <param name="cmd">The <see cref="CommandQueueDX11"/> that will perform the 'get' operation.</param>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="dataStride">The size of the data being retrieved. The default value is 0. 
        /// A value of 0 will force the stride of <see cref="{T}"/> to be automatically calculated, which may cause a tiny performance hit.</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback)
            where T : unmanaged
        {
            if ((Desc.CPUAccessFlags & (uint)CpuAccessFlag.Read) != (uint)CpuAccessFlag.Read)
                throw new GraphicsBufferException(this, "Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            QueueOperation(priority, new BufferGetOperation<T>()
            {
                ByteOffset = byteOffset,
                DestArray = destination,
                DestIndex = startIndex,
                Count = count,
                DataStride = (uint)sizeof(T),
                CompletionCallback = completionCallback,
                SrcBuffer = this,
            });
        }

        /// <summary>Applies any pending changes onto the buffer.</summary>
        /// <param name="context">The graphics pipe to use when process changes.</param>
        /// <param name="forceInitialize">If set to true, the buffer will be initialized if not done so already.</param>
        protected void ApplyChanges(GraphicsCommandQueue context)
        {
            if (_pendingChanges.Count > 0)
            {
                IBufferOperation op = null;
                while (_pendingChanges.TryDequeue(out op))
                    op.Process(context);
            }
        }

        internal void Clear()
        {
            _pendingChanges.Clear();
        }

        internal bool HasBindFlags(BindFlag flag)
        {
            return ((BindFlag)Desc.BindFlags & flag) == flag;
        }

        internal bool HasFlag(CpuAccessFlag flag)
        {
            return ((CpuAccessFlag)Desc.CPUAccessFlags & flag) == flag;
        }

        protected override void OnApply(GraphicsCommandQueue context)
        {
            ApplyChanges(context);
        }

        public override void GraphicsRelease()
        {
            base.GraphicsRelease();

            if (NativePtr != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Desc.ByteWidth);
            }
        }

        /// <summary>Gets the stride (byte size) of each element within the current <see cref="BufferDX11"/>.</summary>
        public uint Stride { get; }

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public uint ByteCapacity { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="BufferDX11"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferFlags Flags { get; }

        /// <summary>Gets the bind flags associated with the buffer.</summary>
        public BindFlag BufferBindFlags => (BindFlag)Desc.BindFlags;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal override ID3D11Buffer* ResourcePtr => _native;

        public override unsafe ID3D11Resource* NativePtr => (ID3D11Resource*)_native;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        public ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Desc.MiscFlags;

        /// <summary>
        /// Gets a value indicating whether the current buffer is a shader resource.
        /// </summary>
        public bool IsShaderResource =>((BindFlag)Desc.BindFlags & BindFlag.ShaderResource) == BindFlag.ShaderResource;

        /// <summary>
        /// Gets a value indicating whether the current buffer has unordered access.
        /// </summary>
        public bool IsUnorderedAccess => ((BindFlag)Desc.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess;
    }
}
