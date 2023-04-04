namespace Molten.Graphics
{
    public class AdapterOutputException : GraphicsDeviceException
    {
        public AdapterOutputException(IDisplayOutput output, string message) : base(output.Device, message)
        {
            Output = output;
        }

        public IDisplayOutput Output { get; private set; }
    }
}
