namespace Molten.Graphics
{
    internal struct TextureCopyLevel : ITextureTask
    {
        public TextureBase Destination;

        public uint SourceLevel;
        public uint SourceSlice;

        public uint DestinationLevel;
        public uint DestinationSlice;

        public unsafe bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {

            uint srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            uint destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            cmd.CopyResourceRegion(texture.NativePtr, srcSub, null, Destination.NativePtr, destSub, Vector3UI.Zero);
            return Destination == texture;
        }
    }
}
