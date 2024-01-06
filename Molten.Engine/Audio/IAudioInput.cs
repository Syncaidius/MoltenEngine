namespace Molten.Audio;

public interface IAudioInput : IAudioDevice
{
    void StartCapture();

    void StopCapture();

    /// <summary>
    /// Reads samples from the capture device.
    /// </summary>
    /// <param name="buffer">The <see cref="AudioBuffer"/> to copy samples into after reading.</param>
    /// <param name="samples">The number of samples to read</param>
    int ReadSamples(AudioBuffer buffer, int samples);

    /// <summary>
    /// Returns the number of captured samples available to read from the current <see cref="IAudioInput"/>.
    /// </summary>
    /// <returns></returns>
    int GetAvailableSamples();

    /// <summary>
    /// Gets or sets frequency at which the current <see cref="IAudioInput"/> will capture audio.
    /// </summary>
    uint Frequency { get; set; }

    /// <summary>
    /// Gets or sets the format in which the current <see cref="IAudioInput"/> will capture audio.
    /// </summary>
    AudioFormat Format { get; set; }

    /// <summary>
    /// Gets the capture-buffer size of the current <see cref="IAudioInput"/>.
    /// </summary>
    int BufferSize { get; set; }

    /// <summary>
    /// Gets whether or not the current <see cref="IAudioInput"/> is recording/capturing audio.
    /// </summary>
    bool IsCapturing { get; }
}
