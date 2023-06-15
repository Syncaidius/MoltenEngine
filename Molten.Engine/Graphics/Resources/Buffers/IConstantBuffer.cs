namespace Molten.Graphics
{
    public interface IConstantBuffer : IGraphicsResource
    {
        string BufferName { get; }

        bool IsDirty { get; set; }
    }
}
