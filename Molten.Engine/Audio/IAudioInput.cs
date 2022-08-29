using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public interface IAudioInput : IAudioDevice
    {

        public AudioDeviceType Type => AudioDeviceType.Input;

        public uint Frequency { get; set; }

        public AudioFormat Format { get; set; }

        public int BufferSize { get; set; }
    }
}
