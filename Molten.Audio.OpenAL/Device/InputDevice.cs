using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    public unsafe class InputDevice : AudioDevice, IAudioInput
    {
        internal InputDevice(AudioServiceAL service, string specifier, bool isDefault) : 
            base(service, specifier, isDefault, AudioDeviceType.Input)
        {
            
        }

        ~InputDevice()
        {
            Dispose();
        }

        internal override void Open()
        {

        }

        internal override void Close()
        {

        }

        protected override void OnTransferTo(AudioDevice other)
        {
            
        }
    }
}
