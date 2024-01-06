namespace Molten.UI;

[Flags]
public enum UIInputRuleFlags
{
    /// <summary>
    /// No input accepted.
    /// </summary>
    None = 0,

    /// <summary>
    /// Input on self is accepted.
    /// </summary>
    Self = 1,

    /// <summary>
    /// Input on child element layers is accepted.
    /// </summary>
    Children = 2,

    /// <summary>
    /// All types of input are accepted.
    /// </summary>
    All = Self | Children,
}
