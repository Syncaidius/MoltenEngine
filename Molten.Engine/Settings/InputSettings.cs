using System.Runtime.Serialization;

namespace Molten.Input
{
    [DataContract]
    public class InputSettings : SettingBank
    {
        internal InputSettings()
        {
            PointerBufferSize = AddSetting("pointer_buffer_size", 300);
            PointerSensitivity = AddSetting("pointer_sensitivity", 1.0f);
            KeyboardBufferSize = AddSetting("kb_buffer_size", 300);
            GamepadBufferSize = AddSetting("gpad_buffer_size", 300);
        }

        /// <summary>Gets or sets the mouse input buffer size.</summary>
        [DataMember]
        public SettingValue<int> PointerBufferSize { get; }

        /// <summary>
        /// Gets or sets the pointing device sensitivity. e.g. mouse, trackball, etc.
        /// </summary>
        public SettingValue<float> PointerSensitivity { get; }

        /// <summary>Gets or sets the keyboard input buffer size.</summary>
        [DataMember]
        public SettingValue<int> KeyboardBufferSize { get; }


        /// <summary>Gets or sets the buffer size of any connected gamepads.</summary>
        [DataMember]
        public SettingValue<int> GamepadBufferSize { get; }
    }
}
