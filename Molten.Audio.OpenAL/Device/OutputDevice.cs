using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    internal unsafe class OutputDevice : AudioDevice, IAudioOutput
    {
        Device* _device;

        internal OutputDevice(AudioServiceAL service, string specifier, bool isDefault) :
            base(service, specifier, isDefault, AudioDeviceType.Output)
        {
        }

        internal override void Open()
        {
            if (_device != null)
                throw new InvalidOperationException("Device is already open");

            _device = Service.Context.OpenDevice(Name);
            ContextError err = Service.Context.GetError(_device);
            if (err != ContextError.NoError)
                Service.Log.Error($"An error occurred while opening device '{Name}': {err}");
            else
                Service.Log.WriteLine($"Opened device '{Name}'");
        }

        internal override void Close()
        {
            if (_device != null)
            {
                Service.Context.CloseDevice(_device);
                _device = null;
                Service.Log.WriteLine($"Closed device '{Name}'");
            }
            else
            {
                Service.Log.Error($"Error closing device '{Name}': Not open");
            }
        }

        protected override void OnTransferTo(AudioDevice other)
        {
            
        }

        internal ref Device* Ptr => ref _device;
    }
}
