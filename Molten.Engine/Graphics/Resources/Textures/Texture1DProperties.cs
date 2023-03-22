namespace Molten.Graphics
{
    public class Texture1DProperties
    {

        public uint MipMapLevels = 1;

        public uint Width = 1;

        public GraphicsFormat Format = GraphicsFormat.R8G8B8A8_UNorm;

        public uint ArraySize = 1;

        public GraphicsResourceFlags Flags = GraphicsResourceFlags.None;

        public string Name = null;
    }
}
