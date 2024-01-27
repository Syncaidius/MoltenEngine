using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11;

/// <summary>A typed, structured buffer. This is the application-equivilent of a typed Buffer and RWBuffer in HLSL. </summary>
/// <typeparam name="T"></typeparam>
internal unsafe class TypedBufferDX11 : BufferDX11
{
    /// <summary>Creates a new instance of <see cref="TypedBufferDX11"/>.</summary>
    /// <param name="device">The graphics device to bind the buffer to.</param>
    /// <param name="format">The format of the typed buffer. Only UInt32, Int32 and Float are allowed.</param>
    /// <param name="numElements"></param>
    /// <param name="flags"></param>
    internal TypedBufferDX11(
        DeviceDX11 device, 
        GraphicsResourceFlags flags, 
        TypedBufferFormat format,
        uint numElements,
        void* initialData,
        uint initialBytes)
        : base(device, GraphicsBufferType.Structured,
              flags,
              GraphicsFormat.Unknown,
              format switch
              {
                  TypedBufferFormat.UInt32 => sizeof(uint),
                  TypedBufferFormat.Int32 => sizeof(int),
                  TypedBufferFormat.Float => sizeof(float)
              }, numElements, 1, initialData, initialBytes)
    {
        TypedFormat = format;
        switch (format)
        {
            case TypedBufferFormat.Float:
                ResourceFormat = GraphicsFormat.R32_Float;
                break;

            case TypedBufferFormat.Int32:
                ResourceFormat = GraphicsFormat.R32_SInt;
                break;

            case TypedBufferFormat.UInt32:
                ResourceFormat = GraphicsFormat.R32_UInt;
                break;
        }
    }

    protected override void CreateViews(DeviceDX11 device, ResourceHandleDX11<ID3D11Buffer> handle)
    {
        if (!Flags.Has(GraphicsResourceFlags.DenyShaderAccess))
        {
            handle.SRV.Desc = new ShaderResourceViewDesc1()
            {
                BufferEx = new BufferexSrv()
                {
                    NumElements = (uint)ElementCount,
                    FirstElement = 0,
                    Flags = 0 // See: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_bufferex_srv_flag
                },
                ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                Format = Format.FormatUnknown,
            };

            handle.SRV.Create();
        }

        // See UAV notes: https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-intro#raw-views-of-buffers
        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
        {
            Handle.UAV.Desc = new UnorderedAccessViewDesc1()
            {
                Format = Format.FormatUnknown,
                ViewDimension = UavDimension.Buffer,
                Buffer = new BufferUav()
                {
                    NumElements = (uint)ElementCount,
                    FirstElement = 0,
                    Flags = 0U, // (uint)BufferUavFlag.None,
                }
            };
            Handle.UAV.Create();
        }
    }

    public TypedBufferFormat TypedFormat { get; }

    public override GraphicsFormat ResourceFormat { get; protected set; }
}
