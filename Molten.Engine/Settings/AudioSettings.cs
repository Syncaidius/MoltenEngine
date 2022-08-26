using Molten.Net;
using System.Runtime.Serialization;

namespace Molten
{
    [DataContract]
    public class AudioSettings : SettingBank
    {
        internal AudioSettings()
        {
            MasterVolume = AddSetting("vol_master", 100f);
            MasterVolume = AddSetting("vol_sfx", 100f);
            MasterVolume = AddSetting("vol_music", 100f);
        }

        /// <summary>
        /// The master audio volume, between 0 and 100.
        /// </summary>
        [DataMember]
        public SettingValue<float> MasterVolume { get; }

        /// <summary>
        /// The SFX (sound effects) volume, between 0 and 100.
        /// </summary>
        [DataMember]
        public SettingValue<float> SfxVolume { get; }

        /// <summary>
        /// The music volume, between 0 and 100.
        /// </summary>
        [DataMember]
        public SettingValue<float> MusicVolume { get; }
    }
}
