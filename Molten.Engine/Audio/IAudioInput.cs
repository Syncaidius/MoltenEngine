using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public interface IAudioInput : IAudioDevice
    {
        void StartCapture();

        void StopCapture();

        /// <summary>
        /// Reads samples from the capture device.
        /// </summary>
        /// <param name="buffer">The <see cref="IAudioBuffer"/> to copy samples into after reading.</param>
        /// <param name="samples">The number of samples to read</param>
        int ReadSamples(IAudioBuffer buffer, int samples);

        /// <summary>
        /// Returns the number of captured samples available to read from the current <see cref="IAudioInput"/>.
        /// </summary>
        /// <returns></returns>
        int GetAvailableSamples();

        AudioDeviceType Type => AudioDeviceType.Input;

        uint Frequency { get; set; }

        AudioFormat Format { get; set; }

        int BufferSize { get; set; }

        /// <summary>
        /// Gets whether or not the current <see cref="IAudioInput"/> is recording/capturing audio.
        /// </summary>
        bool IsCapturing { get; }
    }
}
