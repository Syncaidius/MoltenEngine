using System.Runtime.Serialization;

namespace Molten.Input
{
    [DataContract]
    public class InputSettings : SettingBank
    {
        internal InputSettings()
        {
            PointerBufferSize = AddSetting<int>("mouse_buffer_size", 300);
            KeyboardBufferSize = AddSetting<int>("kb_buffer_size", 300);
            GamepadBufferSize = AddSetting<int>("gpad_buffer_size", 300);
        }

        /// <summary>Gets or sets the mouse input buffer size.</summary>
        [DataMember]
        public SettingValue<int> PointerBufferSize { get; }

        /// <summary>Gets or sets the keyboard input buffer size.</summary>
        [DataMember]
        public SettingValue<int> KeyboardBufferSize { get; }


        /// <summary>Gets or sets the buffer size of any connected gamepads.</summary>
        [DataMember]
        public SettingValue<int> GamepadBufferSize { get; }
    }
}
