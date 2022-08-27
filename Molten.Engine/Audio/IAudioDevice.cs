using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public interface IAudioDevice
    {

        public string Name { get; }

        /// <summary>
        /// Gets whether the current <see cref="IAudioDevice"/> is the default.
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        /// Gets whether the current <see cref="IAudioDevice"/> is set as the current device for Molten.
        /// </summary>
        public bool IsCurrent { get; }

        /// <summary>
        /// Gets the type of the current <see cref="IAudioDevice"/>.
        /// </summary>
        public AudioDeviceType DeviceType { get; }

        /// <summary>
        /// Gets the <see cref="AudioService"/> which manages the current <see cref="IAudioDevice"/>.
        /// </summary>
        public AudioService Service { get; }
    }
}
