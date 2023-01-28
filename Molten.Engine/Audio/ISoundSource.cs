namespace Molten.Audio
{
    /// <summary>
    /// Represents an implementation of an audio source.
    /// </summary>
    public interface ISoundSource : IDisposable
    {
        /// <summary>
        /// Gets a new <see cref="ISoundInstance"/> which uses the current <see cref="ISoundSource"/> as an audio data source.
        /// </summary>
        /// <returns></returns>
        ISoundInstance CreateInstance();

        void CommitBuffer(AudioBuffer buffer);

        void CommitBuffer(AudioBuffer buffer, uint position, int numSamples);

        /// <summary>
        /// Gets the number of active <see cref="ISoundInstance"/>s bound to the current <see cref="ISoundSource"/>.
        /// </summary>
        int InstanceCount { get; }

        /// <summary>
        /// Gets the <see cref="IAudioOutput"/> device which owns the current <see cref="ISoundSource"/>.
        /// </summary>
        IAudioOutput Output { get; }

        /// <summary>
        /// Gets the identifying name of the sound source. This does not have to be unique.
        /// </summary>
        string Name { get; set; }
    }
}
