using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio.OpenAL
{
    public class AudioDevice : IAudioDevice
    {
        AudioServiceAL _service;

        internal AudioDevice(AudioServiceAL service, string specifier, bool isDefault, AudioDeviceType deviceType)
        {
            Name = specifier;
            DeviceType = deviceType;
            IsDefault = IsDefault;
            _service = service;
        }

        public string Name { get; }

        public bool IsDefault { get; }

        public bool IsCurrent { get; }

        public AudioDeviceType DeviceType { get; }

        public AudioServiceAL Service => _service;

        AudioService IAudioDevice.Service => _service;
    }
}
