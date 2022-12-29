using System.Runtime.Serialization;

namespace Molten.Input
{
    [DataContract]
    public class InputSettings : SettingBank
    {
        public InputSettings()
        {
            PointerBufferSize = AddSetting("pointer_buffer_size", 300);
            PointerSensitivity = AddSetting("pointer_sensitivity", 1.0f);
            DoubleClickInterval = AddSetting("double_click_interval", 200);
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

        /// <summary>
        /// Gets or sets the number of milliseconds between clicks for two clicks to be detected as a double-click.
        /// </summary>
        [DataMember]
        public SettingValue<int> DoubleClickInterval { get; }

        /// <summary>Gets or sets the keyboard input buffer size.</summary>
        [DataMember]
        public SettingValue<int> KeyboardBufferSize { get; }


        /// <summary>Gets or sets the buffer size of any connected gamepads.</summary>
        [DataMember]
        public SettingValue<int> GamepadBufferSize { get; }
    }
}
