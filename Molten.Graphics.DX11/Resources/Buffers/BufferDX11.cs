using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Molten.Graphics
{
    public abstract unsafe class BufferDX11 : GraphicsBuffer
    {
        ID3D11Buffer* _native;
        uint _ringPos;
        internal BufferDesc Desc;

        internal BufferDX11(DeviceDX11 device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            BindFlag bindFlags,
            uint stride,
            uint numElements,
            ResourceMiscFlag optionFlags = 0,
            void* initialData = null) : base(device, stride, numElements, flags, type)
        {
            NativeSRV = new SRView(this);
            NativeUAV = new UAView(this);

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
            Desc.Usage = Flags.ToUsageFlags();
            Desc.CPUAccessFlags = (uint)Flags.ToCpuFlags();

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
                NativeSRV.Desc = new ShaderResourceViewDesc1()
                {
                    Buffer = new BufferSrv()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                NativeSRV.Create();
            }

            if (HasBindFlags(BindFlag.UnorderedAccess))
            {
                NativeUAV.Desc = new UnorderedAccessViewDesc1()
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
                NativeUAV.Create();
            }
        }

        /// <summary>Copies all the data in the current <see cref="BufferDX11"/> to the destination <see cref="BufferDX11"/>.</summary>
        /// <param name="cmd">The <see cref="CommandQueueDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="BufferDX11"/> to copy to.</param>
        public override void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, Action<GraphicsResource> completionCallback = null)
        {
            if (SizeInBytes < Desc.ByteWidth)
                throw new GraphicsResourceException(this, "The destination buffer is not large enough.");

            QueueTask(priority, new ResourceCopyTask()
            {
                Destination = destination as BufferDX11,
                CompletionCallback = completionCallback,
            });
        }

        public override void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, 
            Action<GraphicsResource> completionCallback = null)
        {
            QueueTask(priority, new SubResourceCopyTask()
            {
                CompletionCallback = completionCallback,
                DestResource = destination as BufferDX11,
                DestStart = new Vector3UI(destByteOffset, 0, 0),
                SrcRegion = sourceRegion,
            });
        }

        public override void GetStream(GraphicsPriority priority, GraphicsMapType mapType, Action<GraphicsBuffer, GraphicsStream> callback, GraphicsBuffer staging = null)
        {
            QueueTask(priority, new BufferGetStreamTask()
            {
                ByteOffset = 0,
                Staging = staging,
                StreamCallback = callback,
                MapType = mapType,
            });
        }

        public override void SetData<T>(GraphicsPriority priority, T[] data, bool discard, GraphicsBuffer staging = null, Action completeCallback = null)
        {
            SetData(priority, data, 0, (uint)data.Length, discard, 0, staging, completeCallback);
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
        public override void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0,
            GraphicsBuffer staging = null, Action completeCallback = null)
        {
            BufferSetTask<T> op = new BufferSetTask<T>()
            {
                ByteOffset = byteOffset,
                CompletionCallback = completeCallback,
                DestBuffer = this,
                MapType = discard ? GraphicsMapType.Discard : GraphicsMapType.Write,
                ElementCount = elementCount,
                Staging = staging as StagingBufferDX11,
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
        public override void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback)
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
                MapType = GraphicsMapType.Read,
                CompletionCallback = completionCallback,
            });
        }

        internal bool HasBindFlags(BindFlag flag)
        {
            return ((BindFlag)Desc.BindFlags & flag) == flag;
        }

        protected void SetDebugName(string debugName)
        {
            if (!string.IsNullOrWhiteSpace(debugName))
            {
                void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
                ((ID3D11Resource*)Handle)->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
                SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
            }
        }

        public override void GraphicsRelease()
        {
            NativeSRV.Release();
            NativeUAV.Release();

            if (Handle != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Desc.ByteWidth);
            }
        }

        public override bool IsUnorderedAccess => ((BindFlag)Desc.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        internal ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Desc.MiscFlags;

        public override unsafe void* Handle => _native;

        public override unsafe void* SRV => NativeSRV.Ptr;

        public override unsafe void* UAV => NativeUAV.Ptr;

        internal SRView NativeSRV { get; }

        internal UAView NativeUAV { get; }
    }
}
