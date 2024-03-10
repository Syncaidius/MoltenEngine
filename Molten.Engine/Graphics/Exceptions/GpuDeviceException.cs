namespace Molten.Graphics;

public class GpuDeviceException : Exception
{
    public GpuDeviceException(GpuDevice device, string message) : base(message)
    {
        Device = device;
    }

    public GpuDevice Device { get; private set; }
}
