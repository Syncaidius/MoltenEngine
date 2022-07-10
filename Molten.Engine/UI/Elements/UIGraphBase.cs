using Molten.Data;
using Molten.Graphics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public abstract class UIGraphBase : UIElement
    {
        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            InputRules = UIInputRuleFlags.Compound | UIInputRuleFlags.Children;
        }
    }
}
