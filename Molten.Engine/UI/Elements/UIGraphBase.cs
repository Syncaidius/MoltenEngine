namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public abstract class UIGraphBase : UIElement
    {
        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            InputRules = UIInputRuleFlags.Children;
        }
    }
}
