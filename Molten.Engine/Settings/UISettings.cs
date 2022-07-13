using Molten.UI;
using System.Runtime.Serialization;

namespace Molten
{
    public class UISettings : SettingBank
    {
        internal UISettings()
        {
            TooltipDelay = AddSetting("tooltip_delay", 500);
            DragThreshold = AddSetting("drag_threshold", 10f);
            Theme = AddSetting("theme", new UITheme()
            {
                DefaultFontName = "Arial"
            });
        }

        /// <summary>
        /// Gets the setting value for the number of milliseconds the cursor must hover over a <see cref="Molten.UI.UIElement"/> before a tooltip is displayed.
        /// </summary>
        [DataMember]
        public SettingValue<int> TooltipDelay { get; }

        /// <summary>
        /// Gets the setting value for the number of pixels the cursor must be dragged to initiate a drag gesture.
        /// </summary>
        [DataMember]
        public SettingValue<float> DragThreshold { get; }

        /// <summary>
        /// Gets the current <see cref="UITheme"/>. This setting is not serialized when settings are saved to file.
        /// </summary>
        [DataMember]
        public SettingValue<UITheme> Theme { get; }        
       
    }
}
