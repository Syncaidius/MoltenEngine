namespace Molten.Graphics
{
    public class SpriteBatcherException : Exception
    {
        public SpriteBatcherException(SpriteBatcher sb, string message) : base(message)
        {
            Batcher = sb;
        }

        public SpriteBatcher Batcher { get; private set; }
    }
}
