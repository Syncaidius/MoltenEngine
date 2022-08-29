using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    /// <summary>
     /// Represents an implementation of an audio instance.
     /// </summary>
    public interface ISoundInstance : IDisposable
    {
        void Play();

        void Pause();

        void Stop();

        /// <summary>
        /// Clears the queue (if any) and sets the current <see cref="ISoundInstance"/> to play only the provided <see cref="ISoundSource"/>.
        /// </summary>
        /// <param name="source"></param>
        void SetSource(ISoundSource source);

        /// <summary>
        /// Adds the provided <see cref="ISoundSource"/> to the playback queue of the current <see cref="ISoundInstance"/>.
        /// </summary>
        /// <param name="source"></param>
        void QueueSource(ISoundSource source);

        /// <summary>
        /// Queues 
        /// </summary>
        /// <param name="sources">A list of <see cref="ISoundSource"/> to be queued on the current <see cref="ISoundInstance"/>.</param>
        void QueueSources(ISoundSource[] sources);

        /// <summary>
        /// Gets the <see cref="ISoundSource"/> that is being played through the current <see cref="ISoundInstance"/>.
        /// </summary>
        ISoundSource Source { get; }

        /// <summary>
        /// Gets the number of queued <see cref="ISoundSource"/> on the current <see cref="ISoundInstance"/>.
        /// </summary>
        int QueuedSourceCount { get; }

        /// <summary>
        /// Gets the state of the current <see cref="ISoundInstance"/>.
        /// </summary>
        AudioPlaybackState State { get; }

        /// <summary>
        /// Gets or sets whether or not the current <see cref="ISoundInstance"/> shound loop.
        /// </summary>
        bool IsLooping { get; set; }
    }
}
