using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

/// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
internal unsafe class TextureResolve : GraphicsResourceTask<TextureDX11>
{
    public uint SourceArraySlice;

    public uint SourceMipLevel;

    public TextureDX11 Destination;

    public uint DestArraySlice;

    public uint DestMipLevel;

    public override void ClearForPool()
    {
        SourceArraySlice = 0;
        SourceMipLevel = 0;
        Destination = null;
        DestArraySlice = 0;
        DestMipLevel = 0;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        uint subSource = (Resource.MipMapCount * SourceArraySlice) + SourceMipLevel;
        uint subDest = (Destination.MipMapCount * DestArraySlice) + DestMipLevel;

        Destination.Apply(queue);
        Resource.Device.Queue.Ptr->ResolveSubresource((ID3D11Resource*)Destination.Handle, subDest,
            (ID3D11Resource*)Resource.Handle, subSource, Resource.DxgiFormat);

        Destination.Version++;
        return false;
    }
}
