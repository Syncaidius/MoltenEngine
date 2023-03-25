using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public abstract unsafe class BufferDX11 : ResourceDX11<ID3D11Buffer>, IGraphicsBuffer
    {
        ID3D11Buffer* _native;
        uint _ringPos;
        internal BufferDesc Desc;

        internal BufferDX11(DeviceDX11 device,
            GraphicsResourceFlags bufferFlags,
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
            SizeInBytes = Stride * numElements;
            ElementCount = numElements;

            InitializeBuffer( bindFlags, optionFlags, initialData);
            device.ProcessDebugLayerMessages();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialDataPtr">A pointer to data that the buffer should initially be populated with.</param>
        protected virtual void InitializeBuffer(BindFlag bindFlags, ResourceMiscFlag opFlags, void* initialData)
        {
            DeviceDX11 nDevice = Device as DeviceDX11;
            if (Flags.IsImmutable() && initialData == null)
                throw new GraphicsResourceException(this, "Initial data cannot be null when buffer mode is Immutable.");

            Desc = new BufferDesc();
            Desc.ByteWidth = SizeInBytes;
            Desc.StructureByteStride = 0;
            Desc.Usage = GetUsageFlags();
            Desc.CPUAccessFlags = (uint)GetCpuFlags();

            // Only staging allows CPU reads.
            // See for ref: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_usage
            if (Desc.Usage == Usage.Staging)
            {
                Desc.MiscFlags = 0U;
                Desc.BindFlags = 0U;
            }
            else
            {
                Desc.BindFlags = (uint)bindFlags;
                Desc.MiscFlags = (uint)opFlags;

                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    Desc.BindFlags |= (uint)BindFlag.ShaderResource;
            }

            // Ensure structured buffers get the stride info.
            if (Desc.MiscFlags == (uint)ResourceMiscFlag.BufferStructured)
                Desc.StructureByteStride = Stride;

            if (initialData != null)
            {
                SubresourceData srd = new SubresourceData(initialData, SizeInBytes, SizeInBytes);
                nDevice.Ptr->CreateBuffer(ref Desc, ref srd, ref _native);
            }
            else
            {
                nDevice.Ptr->CreateBuffer(ref Desc, null, ref _native);
            }

            Device.AllocateVRAM(SizeInBytes);
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

                SRV.Create(ResourcePtr);
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
                UAV.Create(ResourcePtr);
            }
        }

        /// <summary>Copies all the data in the current <see cref="BufferDX11"/> to the destination <see cref="BufferDX11"/>.</summary>
        /// <param name="cmd">The <see cref="CommandQueueDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="BufferDX11"/> to copy to.</param>
        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, Action<GraphicsResource> completionCallback = null)
        {
            if (SizeInBytes < Desc.ByteWidth)
                throw new GraphicsResourceException(this, "The destination buffer is not large enough.");

            QueueTask(priority, new ResourceCopyTask()
            {
                Destination = destination as BufferDX11,
                CompletionCallback = completionCallback,
            });
        }

        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, 
            Action<GraphicsResource> completionCallback = null)
        {
            QueueTask(priority, new SubResourceCopyTask()
            {
                CompletionCallback = completionCallback,
                DestResource = destination as BufferDX11,
                DestStart = new Vector3UI(destByteOffset, 0, 0),
                SrcRegion = sourceRegion.ToApi(),
            });
        }

        public void GetStream(GraphicsPriority priority, Action<IGraphicsBuffer, RawStream> callback, IStagingBuffer staging = null)
        {
            QueueTask(priority, new BufferGetStreamTask()
            {
                ByteOffset = 0,
                NumElements = ElementCount,
                Staging = staging,
                StreamCallback = callback,
                Stride = Stride
            });
        }

        internal void GetStream(CommandQueueDX11 cmd,
            uint byteOffset,
            uint stride,
            uint elementCount,
            Action<BufferDX11, GraphicsStream> callback,
            StagingBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Desc.Usage == Usage.Dynamic;
            bool isStaged = Desc.Usage == Usage.Staging &&
                (Desc.CPUAccessFlags & (uint)CpuAccessFlag.Write) == (uint)CpuAccessFlag.Write;

            uint numBytes = stride * elementCount;
            GraphicsStream stream;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                if (Flags.Has(GraphicsResourceFlags.Discard))
                {
                    stream = cmd.MapResource(this, 0, byteOffset);
                }
                else if (Flags.Has(GraphicsResourceFlags.Ring))
                {
                    // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                    // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                    if (HasBindFlags(BindFlag.VertexBuffer) || HasBindFlags(BindFlag.IndexBuffer))
                    {
                        if (_ringPos > 0 && _ringPos + numBytes < Desc.ByteWidth)
                        {
                            stream = cmd.MapResource(this, 0, _ringPos);
                            _ringPos += numBytes;
                        }
                        else
                        {
                            stream = cmd.MapResource(this, 0, 0);
                            stream.Position = 0;
                            _ringPos = numBytes;
                        }
                    }
                    else
                    {
                        stream = cmd.MapResource(this, 0, byteOffset);
                    }
                }
                else
                {
                    stream = cmd.MapResource(this, 0, 0);
                }

                callback(this, stream);
                stream.Dispose();
            }
            else
            {
                // Write to the provided staging buffer instead.
                if (staging == null)
                    throw new GraphicsResourceException(this, "Staging buffer required. Non-dynamic/staged buffers require a staging buffer to access data.");

                isDynamic = staging.Desc.Usage == Usage.Dynamic;
                isStaged = staging.Desc.Usage == Usage.Staging;

                if (!isDynamic && !isStaged)
                    throw new GraphicsResourceException(this, "The provided staging buffer is invalid. Must be either dynamic or staged.");

                if (staging.Desc.ByteWidth < numBytes)
                    throw new GraphicsResourceException(this, $"The provided staging buffer is not large enough ({staging.Desc.ByteWidth} bytes) to fit the provided data ({numBytes} bytes).");

                // Write updated data into buffer
                if (isDynamic) // Always discard staging buffer data, since the old data is no longer needed after it's been copied to it's target resource.
                {
                    stream = cmd.MapResource(staging, 0, 0);
                }
                else
                {
                    stream = cmd.MapResource(staging, 0, 0);
                }

                callback(staging, stream);
                stream.Dispose();

                Box stagingRegion = new Box()
                {
                    Left = 0,
                    Right = numBytes,
                    Back = 1,
                    Bottom = 1,
                };
                cmd.CopyResourceRegion(staging.ResourcePtr, 0, ref stagingRegion,
                    ResourcePtr, 0, new Vector3UI(byteOffset, 0, 0));
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
            BufferSetTask<T> op = new BufferSetTask<T>()
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
                op.Process(Device.Cmd, this);
            }
            else
            {
                // Only copy the part we need from the source data, starting from startIndex.
                op.Data = new T[data.Length];
                op.DataStartIndex = 0;
                Array.Copy(data, (int)startIndex, op.Data, 0, elementCount);
                QueueTask(priority, op);
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
                throw new GraphicsResourceException(this, "Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            QueueTask(priority, new BufferGetTask<T>()
            {
                ByteOffset = byteOffset,
                DestArray = destination,
                DestIndex = startIndex,
                Count = count,
                CompletionCallback = completionCallback,
            });
        }

        internal bool HasBindFlags(BindFlag flag)
        {
            return ((BindFlag)Desc.BindFlags & flag) == flag;
        }

        internal bool HasFlag(CpuAccessFlag flag)
        {
            return ((CpuAccessFlag)Desc.CPUAccessFlags & flag) == flag;
        }

        public override void GraphicsRelease()
        {
            base.GraphicsRelease();

            if (ResourcePtr != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Desc.ByteWidth);
            }
        }

        /// <summary>Gets the stride (byte size) of each element within the current <see cref="BufferDX11"/>.</summary>
        public uint Stride { get; }

        /// <summary>Gets the capacity of the buffer, in bytes.</summary>
        public override uint SizeInBytes { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="BufferDX11"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public override GraphicsResourceFlags Flags { get; }

        internal override Usage UsageFlags => Desc.Usage;

        public override bool IsUnorderedAccess => ((BindFlag)Desc.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal override ID3D11Buffer* NativePtr => _native;

        internal override unsafe ID3D11Resource* ResourcePtr => (ID3D11Resource*)_native;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        internal ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Desc.MiscFlags;
    }
}
