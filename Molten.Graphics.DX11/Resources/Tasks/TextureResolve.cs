using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

/// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
internal unsafe class TextureResolve : GraphicsTask<TextureResolve>
{
    public TextureDX11 Source;

    public uint SourceArraySlice;

    public uint SourceMipLevel;

    public TextureDX11 Destination;

    public uint DestArraySlice;

    public uint DestMipLevel;

    public override void ClearForPool()
    {
        Source = null;
        Destination = null;
    }

    public override void Process(RenderService renderer)
    {
        uint subSource = (Source.MipMapCount * SourceArraySlice) + SourceMipLevel;
        uint subDest = (Destination.MipMapCount * DestArraySlice) + DestMipLevel;

        RendererDX11 dx11Renderer = renderer as RendererDX11;
        Destination.Apply(dx11Renderer.NativeDevice.Queue);
        dx11Renderer.NativeDevice.Queue.Ptr->ResolveSubresource((ID3D11Resource*)Destination.Handle, subDest,
            (ID3D11Resource*)Source.Handle, subSource, Source.DxgiFormat);
        Recycle(this);
    }
}
