namespace Molten.Audio
{
    public interface IAudioDevice
    {
        /// <summary>
        /// Gets the name of the current <see cref="IAudioDevice"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether the current <see cref="IAudioDevice"/> is the default.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// Gets whether the current <see cref="IAudioDevice"/> is set as the current device for Molten.
        /// </summary>
        bool IsCurrent { get; }

        /// <summary>
        /// Gets the type of the current <see cref="IAudioDevice"/>.
        /// </summary>
        AudioDeviceType DeviceType { get; }

        /// <summary>
        /// Gets the <see cref="AudioService"/> which manages the current <see cref="IAudioDevice"/>.
        /// </summary>
        AudioService Service { get; }
    }
}
