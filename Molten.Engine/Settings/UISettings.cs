using System.Runtime.Serialization;

namespace Molten
{
    public class UISettings : SettingBank
    {
        internal UISettings()
        {
            TooltipDelay = AddSetting("tooltip_delay", 500);
            DragThreshold = AddSetting("drag_threshold", 10f);
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

        public static readonly Color DefaultTextColor = Color.White;

        public static readonly Color DefaultBackgroundColor = new Color(40, 40, 150, 200);

        public static readonly Color DefaultBorderColor = new Color(80, 80, 190);
    }
}
