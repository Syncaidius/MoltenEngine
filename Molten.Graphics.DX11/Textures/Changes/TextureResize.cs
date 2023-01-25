using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal struct TextureResize : ITextureTask
    {
        public uint NewWidth;

        public uint NewHeight;

        public uint NewDepth;

        public uint NewMipMapCount;

        public uint NewArraySize;

        public Format NewFormat;

        public bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {
            texture.SetSizeInternal(NewWidth, NewHeight, NewDepth, NewMipMapCount, NewArraySize, NewFormat);
            return true;
        }
    }
}
