namespace Molten.Graphics
{
    public class GraphicsObjectException : Exception
    {
        public GraphicsObjectException(GraphicsObject obj, string message) : base(message)
        {
            Object = obj;
        }

        public GraphicsObject Object { get; private set; }
    }
}
