using System.Runtime.Serialization;

namespace Molten.Input
{
    [DataContract]
    public class InputSettings : SettingBank
    {
        public InputSettings()
        {
            TouchBufferSize = AddSetting<int>("touch_buffer_size", 128);
            MouseBufferSize = AddSetting<int>("touch_buffer_size", 256);
            KeyboardBufferSize = AddSetting<int>("touch_buffer_size", 256);
        }

        /// <summary>Gets or sets the touch device sample buffer size.</summary>
        [DataMember]
        public SettingValue<int> TouchBufferSize { get; }

        /// <summary>Gets or sets the touch device sample buffer size.</summary>
        [DataMember]
        public SettingValue<int> MouseBufferSize { get; }

        /// <summary>Gets or sets the touch device sample buffer size.</summary>
        [DataMember]
        public SettingValue<int> KeyboardBufferSize { get; }
    }
}
