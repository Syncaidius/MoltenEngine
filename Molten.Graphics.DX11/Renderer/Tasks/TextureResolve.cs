namespace Molten.Graphics
{
    /// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
    internal unsafe class TextureResolve : RendererTask<TextureResolve>
    {
        public TextureBase Source;

        public uint SourceArraySlice;

        public uint SourceMipLevel;

        public TextureBase Destination;

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
            Destination.Apply(dx11Renderer.Device.Cmd);
            (renderer as RendererDX11).Device.Cmd.Native->ResolveSubresource(Destination.NativePtr, subDest,
                Source.NativePtr, subSource, Source.DxgiFormat);
            Recycle(this);
        }
    }
}
