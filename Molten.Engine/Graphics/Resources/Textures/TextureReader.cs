namespace Molten.Graphics;

public abstract class TextureReader : IDisposable
{
    public abstract TextureData Read(BinaryReader reader, Logger log, string filename = null);

    public virtual void Dispose() { }
}
