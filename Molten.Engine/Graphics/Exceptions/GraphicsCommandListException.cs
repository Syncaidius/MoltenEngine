namespace Molten.Graphics;

public class GraphicsCommandListException : Exception
{
    public GraphicsCommandListException(GraphicsCommandList list, string message) : base(message)
    {
        List = list;
    }

    public GraphicsCommandList List { get; }
}
