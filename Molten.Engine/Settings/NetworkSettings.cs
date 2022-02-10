using Molten.Net;
using System.Runtime.Serialization;

namespace Molten
{
    [DataContract]
    public class NetworkSettings : SettingBank
    {
        internal NetworkSettings()
        {
            Port = AddSetting<int>("net_port", 6113);
        }

        /// <summary>
        /// The start-up application mode of a <see cref="NetworkManager"/>.
        /// </summary>
        public NetworkMode Mode { get; set; } = NetworkMode.Client;

        /// <summary>
        /// The port to bind a network manager to.
        /// </summary>
        [DataMember]
        public SettingValue<int> Port { get; }
    }
}
