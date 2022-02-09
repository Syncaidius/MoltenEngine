namespace Molten.Graphics
{
    public class AdapterOutputException : AdapterException
    {
        public AdapterOutputException(IDisplayOutput output, string message) : base(output.Adapter, message)
        {
            Output = output;
        }

        public IDisplayOutput Output { get; private set; }
    }
}
