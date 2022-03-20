namespace Molten.Graphics
{
    public abstract class TextureWriter : EngineObject
    {
        public abstract void WriteData(Stream stream, TextureData data, Logger log, string filename = null);
    }
}
