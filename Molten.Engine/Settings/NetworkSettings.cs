using System.Runtime.Serialization;

namespace Molten;

[DataContract]
public class NetworkSettings : SettingBank
{
    public NetworkSettings()
    {
        Port = AddSetting<int>("net_port", 6113);
    }

    /// <summary>
    /// The port to bind a network manager to.
    /// </summary>
    [DataMember]
    public SettingValue<int> Port { get; }
}
