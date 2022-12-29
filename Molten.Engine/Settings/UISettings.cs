using Molten.UI;
using System.Runtime.Serialization;

namespace Molten
{
    public class UISettings : SettingBank
    {
        public UISettings()
        {
            TooltipDelay = AddSetting("tooltip_delay", 500);
            DragThreshold = AddSetting("drag_threshold", 10f);
            DefaultFontName = AddSetting("default_font", "Arial");
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
        /// Gets the default font that should be used for text rendering.
        /// </summary>
        [DataMember]
        public SettingValue<string> DefaultFontName { get; }

        /// <summary>
        /// Gets or sets the default <see cref="UITextParser"/> for <see cref="UITextBox"/>-based elements.
        /// </summary>
        public UITextParser DefaultTextParser { get; set; } = new UIDefaultTextParser();
       
    }
}
