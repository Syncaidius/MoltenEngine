namespace Molten.Audio;

public enum AudioPlaybackState
{
    /// <summary>
    /// The instance is stopped.
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// The instance is playing.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// The instance was paused.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// The instance was disposed.
    /// </summary>
    Disposed = 3,
}
