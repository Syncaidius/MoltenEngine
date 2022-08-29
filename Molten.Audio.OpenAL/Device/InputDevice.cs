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

        public void StartCapture()
        {
            if (_isCapturing)
                return;

            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            _capture.CaptureStart(Ptr);

            ContextError result = Service.Alc.GetError(null);
            if (result != ContextError.NoError)
                Service.Log.Error($"Failed to start capture on '{Name}': {Service.GetErrorMessage(result)}");
            else
                _isCapturing = true;
        }

        public void StopCapture()
        {
            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            _capture.CaptureStop(Ptr);

            ContextError result = Service.Alc.GetError(null);
            if (result != ContextError.NoError)
                Service.Log.Error($"Failed to stop capture on '{Name}': {Service.GetErrorMessage(result)}");

            _isCapturing = false;
        }

        public unsafe int ReadSamples(IAudioBuffer buffer, int sampleCount)
        {
            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            int available = 0;
            Service.Alc.GetContextProperty(Ptr, (GetContextInteger)GetCaptureContextInteger.CaptureSamples, 1, &available);

            sampleCount = MathHelper.Clamp(sampleCount, available, _bufferSize);

            AudioBuffer alBuffer = buffer as AudioBuffer;
            uint remainingCapacity = alBuffer.Size - alBuffer.WritePosition;

            // Read straight into the buffer if we have capacity
            if (remainingCapacity >= sampleCount)
            {
                _capture.CaptureSamples(Ptr, alBuffer.PtrWrite, sampleCount);
                alBuffer.WritePosition += (uint)sampleCount;
            }
            else
            {
                // Fill to the end of the buffer, then put the rest back at the start of the buffer.
                _capture.CaptureSamples(Ptr, alBuffer.PtrWrite, (int)remainingCapacity);
                uint remainingToWrite = (uint)sampleCount - remainingCapacity;

                // Go back to the start of the buffer and write the remaining data.
                alBuffer.WritePosition = 0;
                _capture.CaptureSamples(Ptr, alBuffer.PtrWrite, (int)remainingToWrite);
                alBuffer.WritePosition += remainingToWrite;
            }

            return sampleCount;
        }

        public int GetAvailableSamples()
        {
            if (Ptr == null)
                throw new AudioDeviceException(this, "Input device is not opened or current");

            int available = 0;
            Service.Alc.GetContextProperty(Ptr, (GetContextInteger)GetCaptureContextInteger.CaptureSamples, 1, &available);

            ContextError result = Service.Alc.GetError(null);
            if (result != ContextError.NoError)
                Service.Log.Error($"Failed retrieve capture samples for '{Name}': {Service.GetErrorMessage(result)}");

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
