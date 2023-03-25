namespace Molten.Graphics
{
    /// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
    /// <seealso cref="IDisposable" />
    public interface IGraphicsResource : IGraphicsObject
    {
        GraphicsResourceFlags Flags { get; }
    }
}
