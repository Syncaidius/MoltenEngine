using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio.OpenAL
{
    public class InputDevice : AudioDevice, IAudioInput
    {
        internal InputDevice(AudioServiceAL service, string specifier, bool isDefault) : 
            base(service, specifier, isDefault, AudioDeviceType.Input)
        {
        }
    }
}
