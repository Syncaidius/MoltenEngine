namespace Molten.Graphics
{
    public class DisplayOutputException : GraphicsDeviceException
    {
        public DisplayOutputException(IDisplayOutput output, string message) : base(output.Device, message)
        {
            Output = output;
        }

        public IDisplayOutput Output { get; private set; }
    }
}
