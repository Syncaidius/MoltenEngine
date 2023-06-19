using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    /// <summary>A raw byte-addressed buffer. In HLSL these are intended for use only with unsigned 32-bit integer elements (UInt32). 
    /// <para>If the values you want to store in the buffer, are not UInt32, you could use a HLSL function such as <see cref="asfloat"/>.</para></summary>
    internal unsafe class RawBufferDX11 : BufferDX11
    {
        /// <summary>Creates a new instance of <see cref="RawBufferDX11"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="flags"></param>
        /// <param name="numElements"></param>
        /// <param name="shaderResource"></param>
        /// <param name="unorderedAccess">If true, the buffer is given Read-Write access and a UAV is created for it. This is known as an RWStructuredBuffer in HLSL.</param>
        internal RawBufferDX11(
            DeviceDX11 device,
            GraphicsResourceFlags flags,
            uint numElements,
            void* initialData,
            uint initialBytes)
            : base(device, GraphicsBufferType.ByteAddress, flags, GraphicsFormat.Unknown, sizeof(uint), numElements, initialData, initialBytes)
        {
            
        }

        protected override void CreateViews(DeviceDX11 device, ResourceHandleDX11<ID3D11Buffer> handle, ResourceHandleDX11<ID3D11Buffer> initialHandle)
        {
            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
            {
                handle.SRV.Desc = new ShaderResourceViewDesc1()
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

                handle.SRV.Create();
            }

            // See UAV notes: https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-intro#raw-views-of-buffers
            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            {
                handle.UAV.Desc = new UnorderedAccessViewDesc1()
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
                handle.UAV.Create();
            }
        }
    }
}
