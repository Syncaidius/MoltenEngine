using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A typed, structured buffer. This is the application-equivilent of a typed Buffer and RWBuffer in HLSL. </summary>
    /// <typeparam name="T"></typeparam>
    internal unsafe class TypedBuffer : BufferDX11
    {
        /// <summary>Creates a new instance of <see cref="TypedBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="format">The format of the typed buffer. Only UInt32, Int32 and Float are allowed.</param>
        /// <param name="numElements"></param>
        /// <param name="shaderResource"></param>
        /// <param name="flags"></param>
        /// <param name="unorderedAccess">If true, the buffer is given Read-Write access and a UAV is created for it. This is known as an RWStructuredBuffer in HLSL.</param>
        public TypedBuffer(
            DeviceDX11 device, 
            GraphicsResourceFlags flags, 
            TypedBufferFormat format,
            uint numElements,
            bool unorderedAccess = false, 
            bool shaderResource = true,
            void* initialData = null)
            : base(device, GraphicsBufferType.Structured,
                  flags,
                  (shaderResource ? BindFlag.ShaderResource : 0) | (unorderedAccess ? BindFlag.UnorderedAccess : 0), 
                  format switch
                  {
                      TypedBufferFormat.UInt32 => sizeof(uint),
                      TypedBufferFormat.Int32 => sizeof(int),
                      TypedBufferFormat.Float => sizeof(float)
                  }, numElements, 
                  ResourceMiscFlag.BufferStructured, initialData)
        {
            TypedFormat = format;
        }

        protected override void CreateResources()
        {
            if (HasBindFlags(BindFlag.ShaderResource))
            {
                NativeSRV.Desc = new ShaderResourceViewDesc1()
                {
                    BufferEx = new BufferexSrv()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = 0 // See: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_bufferex_srv_flag
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                NativeSRV.Create();
            }

            // See UAV notes: https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-intro#raw-views-of-buffers
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
                        Flags = (uint)BufferUavFlag.None,
                    }
                };
                NativeUAV.Create();
            }
        }

        public TypedBufferFormat TypedFormat { get; }
    }
}
