using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.EXT;

namespace Molten.Audio.OpenAL
{
    public unsafe class InputDevice : AudioDevice, IAudioInput
    {
        Capture _capture;
        uint _frequency;
        int _bufferSize;
        AudioFormat _format;
        bool _recreate;

        internal InputDevice(
            AudioServiceAL service, 
            string specifier, 
            bool isDefault, 
            uint frequency = 22050, 
            int bufferSize = 22050, 
            AudioFormat format = AudioFormat.Mono8) : 
            base(service, specifier, isDefault, AudioDeviceType.Input)
        {
            _frequency = frequency;
            _format = format;
            _bufferSize = bufferSize;

            TryGetExtension(out _capture);
        }

        ~InputDevice()
        {
            OnClose();
        }


        private void ReinitializeDevice()
        {

        }

        protected override ContextError OnOpen()
        {
            Ptr = _capture.CaptureOpenDevice(Name, _frequency, _format, _bufferSize);
            return Service.Alc.GetError(null);
        }

        protected override ContextError OnClose()
        {
             _capture.CaptureCloseDevice(Ptr);
            return Service.Alc.GetError(null);
        }

        protected override void OnTransferTo(AudioDevice other)
        {
            
        }

        protected override void OnUpdate(Timing time)
        {
            // Do we need to re-create the device?
            if (_recreate)
            {
                ReinitializeDevice();
                _recreate = false;
            }
        }

        public uint Frequency
        {
            get => _frequency;
            set
            {
                if(_frequency != value)
                {
                    _frequency = value;
                    _recreate = true;
                }
            }
        }

        public AudioFormat Format
        {
            get => _format;
            set
            {
                if(_format != value)
                {
                    _format = value;
                    _recreate = true;
                }
            }
        }

        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                if(_bufferSize != value)
                {
                    _bufferSize = value;
                    _recreate = true;
                }
            }
        }
    }
}
