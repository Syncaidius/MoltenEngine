using System.Runtime.Serialization;

namespace Molten.Input
{
    [DataContract]
    public class InputSettings : SettingBank
    {
        public InputSettings()
        {
            TouchBufferSize = AddSetting<int>("touch_buffer_size", 256);
            MouseBufferSize = AddSetting<int>("mouse_buffer_size", 300);
            KeyboardBufferSize = AddSetting<int>("kb_buffer_size", 300);
            GamepadBufferSize = AddSetting<int>("gpad_buffer_size", 300);
        }

        /// <summary>Gets or sets the touch device sample buffer size.</summary>
        [DataMember]
        public SettingValue<int> TouchBufferSize { get; }

        /// <summary>Gets or sets the mouse input buffer size.</summary>
        [DataMember]
        public SettingValue<int> MouseBufferSize { get; }

        /// <summary>Gets or sets the keyboard input buffer size.</summary>
        [DataMember]
        public SettingValue<int> KeyboardBufferSize { get; }


        /// <summary>Gets or sets the buffer size of any connected gamepads.</summary>
        [DataMember]
        public SettingValue<int> GamepadBufferSize { get; }
    }
}
