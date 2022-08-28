using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    public unsafe abstract class AudioDevice : IAudioDevice, IDisposable
    {
        AudioServiceAL _service;

        internal AudioDevice(AudioServiceAL service, string specifier, bool isDefault, AudioDeviceType deviceType)
        {
            Name = specifier;
            DeviceType = deviceType;
            IsDefault = isDefault;
            _service = service;
        }

        ~AudioDevice()
        {
            Close();
        }

        internal void TransferTo(AudioDevice other)
        {
            if (other.Service != Service)
                throw new Exception("Devices must be from the same audio service");

            if (other.DeviceType != DeviceType)
                throw new Exception("Device types must be identical");

            if (other == this)
                throw new InvalidOperationException("An audio device cannot be transferred to itself");

            OnTransferTo(other);
        }

        internal abstract void Open();

        internal abstract void Close();

        /// <summary>
        /// Invoked when the current <see cref="AudioDevice"/> state needs to be transferred to another <see cref="AudioDevice"/> of the same <see cref="DeviceType"/>.
        /// </summary>
        /// <param name="other"></param>
        protected abstract void OnTransferTo(AudioDevice other);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }

        public string Name { get; }

        public bool IsDefault { get; }

        public bool IsCurrent { get; }

        public AudioDeviceType DeviceType { get; }

        public AudioServiceAL Service => _service;

        AudioService IAudioDevice.Service => _service;
    }
}
