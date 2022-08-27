using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio.OpenAL
{
    internal class OutputDevice : AudioDevice, IAudioOutput
    {
        internal OutputDevice(AudioServiceAL service, string specifier, bool isDefault) :
            base(service, specifier, isDefault, AudioDeviceType.Output)
        {
        }
    }
}
