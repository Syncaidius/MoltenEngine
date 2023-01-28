using System.Runtime.Serialization;

namespace Molten
{
    [DataContract]
    public class AudioSettings : SettingBank
    {
        public AudioSettings()
        {
            MasterVolume = AddSetting("vol_master", 100f);
            SfxVolume = AddSetting("vol_sfx", 100f);
            MusicVolume = AddSetting("vol_music", 100f);
            InputDevice = AddSetting<string>("device_input");
            OutputDevice = AddSetting<string>("device_input");
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

        /// <summary>
        /// The audio input/capture device to use. If the device is no longer detected, the default one will be used instead.
        /// </summary>  
        [DataMember]
        public SettingValue<string> InputDevice { get; }

        /// <summary>
        /// The audio output device to use. If the device is no longer detected, the default one will be used instead.
        /// </summary>
        [DataMember]
        public SettingValue<string> OutputDevice { get; }
    }
}
