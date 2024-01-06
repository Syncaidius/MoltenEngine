namespace Molten.UI;

public class UIStyleValue
{
    internal Dictionary<UIElementState, object> Values = new Dictionary<UIElementState, object>();

    /// <summary>
    /// Gets the parent style which owns the current <see cref="UIStyleValue"/>.
    /// </summary>
    internal UIStyle Style { get; }

    internal UIStyleValue(UIStyle parentStyle, object defaultValue)
    {
        Style = parentStyle;
        Values[UIElementState.Default] = defaultValue;
    }

    public object this[UIElementState state]
    {
        get
        {
            if (state != UIElementState.Default & Values.TryGetValue(state, out object val))
                return val;
            else
                return Values[UIElementState.Default];
        }

        set => Values[state] = value;
    }
}
