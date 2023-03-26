using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A raw byte-addressed buffer. In HLSL these are intended for use only with unsigned 32-bit integer elements (UInt32). 
    /// <para>If the values you want to store in the buffer, are not UInt32, you could use a HLSL function such as <see cref="asfloat"/>.</para></summary>
    internal unsafe class RawBufferDX11 : BufferDX11
    {
        /// <summary>Creates a new instance of <see cref="StructuredBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="flags"></param>
        /// <param name="numElements"></param>
        /// <param name="shaderResource"></param>
        /// <param name="unorderedAccess">If true, the buffer is given Read-Write access and a UAV is created for it. This is known as an RWStructuredBuffer in HLSL.</param>
        public RawBufferDX11(
            DeviceDX11 device,
            GraphicsResourceFlags flags,
            uint numElements,
            bool unorderedAccess = false,
            bool shaderResource = true,
            void* initialData = null)
            : base(device,
                  flags,
                  (shaderResource ? BindFlag.ShaderResource : 0) | (unorderedAccess ? BindFlag.UnorderedAccess : 0),
                  sizeof(uint), numElements,
                  ResourceMiscFlag.BufferStructured | ResourceMiscFlag.BufferAllowRawViews, initialData)
        {
            
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
                        Flags = (uint)BufferexSrvFlag.Raw // See: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_bufferex_srv_flag
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBufferex,
                    Format = Format.FormatR32Typeless, // TODO we need to set it to R32Typeless?
                };

                NativeSRV.Create();
            }

            // See UAV notes: https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-intro#raw-views-of-buffers
            if (HasBindFlags(BindFlag.UnorderedAccess))
            {
                NativeUAV.Desc = new UnorderedAccessViewDesc1()
                {
                    // Raw UAVs require the format to be DXGI_FORMAT_R32_TYPELESS.
                    // See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_buffer_uav_flag
                    Format = Format.FormatR32Typeless,
                    ViewDimension = UavDimension.Buffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = (uint)BufferUavFlag.Raw,
                    },
                };
                NativeUAV.Create();
            }
        }
    }
}
