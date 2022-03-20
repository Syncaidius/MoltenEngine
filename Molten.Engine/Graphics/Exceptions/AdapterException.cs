namespace Molten.Graphics
{
    public class AdapterException : Exception
    {
        public AdapterException(IDisplayAdapter adapter, string message) : base(message)
        {
            Adapter = adapter;
        }

        public IDisplayAdapter Adapter { get; private set; }
    }
}
