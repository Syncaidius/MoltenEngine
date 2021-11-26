using System.Runtime.Serialization;

namespace Molten.Net
{
    [DataContract]
    public class NetworkSettings : SettingBank
    {
        public NetworkSettings()
        {
            Port = AddSetting<int>("net_port", 6113);
        }

        /// <summary>
        /// The start-up application mode of a <see cref="NetworkManager"/>.
        /// </summary>
        public NetworkMode NetMode { get; set; } = NetworkMode.Client;

        /// <summary>
        /// The port to bind a network manager to.
        /// </summary>
        [DataMember]
        public SettingValue<int> Port { get; }
    }
}
