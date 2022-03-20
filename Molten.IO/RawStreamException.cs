namespace Molten.IO
{
    public class RawStreamException : Exception
    {
        internal RawStreamException(RawStream stream, string message) : base(message)
        {
            Stream = stream;
        }

        public RawStream Stream { get; }
    }
}
