using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class UISettings : SettingBank
    {
        public UISettings()
        {
            TooltipDelay = AddSetting<int>("tooltip_delay", 500);
            DragThreshold = AddSetting<float>("drag_threshold", 10);
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
    }
}
