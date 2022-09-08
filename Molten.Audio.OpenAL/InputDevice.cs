using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
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
        bool _isCapturing;

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
            if(Ptr != null)
            {
                OnClose();
                OnOpen();
                Service.Log.WriteLine($"Re-initialized input device '{Name}'");
            }
        }

        protected override bool OnOpen()
        {
            Ptr = _capture.CaptureOpenDevice(Name, _frequency, _format, _bufferSize);
            return !CheckAlcError($"An error occurred while closing input device for '{Name}'");
        }

        protected override void OnClose()
        {
            _capture.CaptureCloseDevice(Ptr);
            CheckAlcError($"An error occurred while closing input device for '{Name}'");
        }

        protected override void OnTransferTo(AudioDevice other)
        {
            
        }

        public void StartCapture()
        {
            if (_isCapturing)
                return;

            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            _capture.CaptureStart(Ptr);
            _isCapturing = !CheckAlcError($"Failed to start capture on '{Name}'");
        }

        public void StopCapture()
        {
            if (!_isCapturing)
                return;

            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            _capture.CaptureStop(Ptr);
            CheckAlcError($"Failed to stop capture on '{Name}'");
            _isCapturing = false;
        }

        public unsafe int ReadSamples(AudioBuffer buffer, int sampleCount)
        {
            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            int available = 0;
            Service.Alc.GetContextProperty(Ptr, (GetContextInteger)GetCaptureContextInteger.CaptureSamples, 1, &available);
            if (CheckAlcError($"Unable to retrieve captured sample count from '{Name}'", Ptr))
                return 0;

            sampleCount = MathHelper.Min(MathHelper.Min(available, sampleCount), _bufferSize); 
            uint remainingCapacity = buffer.Size - buffer.WritePosition;

            // Read straight into the buffer if we have capacity
            if (remainingCapacity >= sampleCount)
            {
                _capture.CaptureSamples(Ptr, buffer.PtrWrite, sampleCount);
                buffer.WritePosition += (uint)sampleCount;
            }
            else
            {
                // Fill to the end of the buffer, then put the rest back at the start of the buffer.
                _capture.CaptureSamples(Ptr, buffer.PtrWrite, (int)remainingCapacity);
                uint remainingToWrite = (uint)sampleCount - remainingCapacity;

                // Go back to the start of the buffer and write the remaining data.
                buffer.WritePosition = 0;
                _capture.CaptureSamples(Ptr, buffer.PtrWrite, (int)remainingToWrite);
                buffer.WritePosition += remainingToWrite;
            }

            return sampleCount;
        }

        public int GetAvailableSamples()
        {
            if (Ptr == null)
                return 0;

            int available = 0;
            Service.Alc.GetContextProperty(Ptr, (GetContextInteger)GetCaptureContextInteger.CaptureSamples, 1, &available);
            if (CheckAlcError($"Failed to get capture samples count for '{Name}'"))
                return 0;
            else
                return available;
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

        public bool IsCapturing => _isCapturing;
    }
}
