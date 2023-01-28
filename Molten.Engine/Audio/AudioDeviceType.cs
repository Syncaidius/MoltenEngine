namespace Molten.Audio
{
    /// <summary>
    /// Represents the type of a <see cref="IAudioDevice"/>.
    /// </summary>
    public enum AudioDeviceType
    {
        /// <summary>
        /// The device handles audio input, such as a microphone or line-in source.
        /// </summary>
        Input = 0,

        /// <summary>
        /// The device handles audio output, such as speakers or headphones.
        /// </summary>
        Output = 1,
    }
}
