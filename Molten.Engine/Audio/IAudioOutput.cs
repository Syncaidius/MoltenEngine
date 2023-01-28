namespace Molten.Audio
{
    public interface IAudioOutput : IAudioDevice
    {
        /// <summary>
        /// Creates a new <see cref="ISoundSource"/> which can be pre-buffered with a <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="dataBuffer">The <see cref="AudioBuffer"/> containing the data to be pre-buffered. Null to create an empty source.</param>
        /// <returns></returns>
        ISoundSource CreateSoundSource(AudioBuffer dataBuffer = null);
    }
}
