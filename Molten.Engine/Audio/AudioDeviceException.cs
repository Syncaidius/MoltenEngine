namespace Molten.Audio;

public class AudioDeviceException : Exception
{
    public AudioDeviceException(IAudioDevice device, string message) : base(message)
    {
        DeviceName = device.Name;
        DeviceType = device.DeviceType;
    }

    /// <summary>
    /// Gets the name of the device that caused the exception.
    /// </summary>
    public string DeviceName { get; }

    /// <summary>
    /// Gets the type of the device that caused the exception.
    /// </summary>
    public AudioDeviceType DeviceType { get; }
}
