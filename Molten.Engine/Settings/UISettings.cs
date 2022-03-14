using System.Runtime.Serialization;

namespace Molten
{
    public class UISettings : SettingBank
    {
        internal UISettings()
        {
            TooltipDelay = AddSetting("tooltip_delay", 500);
            DragThreshold = AddSetting("drag_threshold", 10f);
            
            DefaultTextColor = AddSetting("color_text", Color.White);
            DefaultBackgroundColor = AddSetting("color_bg", new Color(40, 40, 150, 200));
            DefaultBorderColor = AddSetting("color_border", new Color(80, 80, 190));
        }

        /// <summary>
        /// Gets the setting value for the number of milliseconds the cursor must hover over a <see cref="Molten.UI.UIComponent"/> before a tooltip is displayed.
        /// </summary>
        [DataMember]
        public SettingValue<int> TooltipDelay { get; }

        /// <summary>
        /// Gets the setting value for the number of pixels the cursor must be dragged to initiate a drag gesture.
        /// </summary>
        [DataMember]
        public SettingValue<float> DragThreshold { get; }

        [DataMember]
        public SettingValue<Color> DefaultTextColor { get; }

        [DataMember]
        public SettingValue<Color> DefaultBackgroundColor { get; }

        [DataMember]
        public SettingValue<Color> DefaultBorderColor { get; }
    }
}
