namespace Molten.Input;

public enum KeyboardKeyType
{
    /// <summary>
    /// A normal key press with no specific purpose or intention.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// The key represents a character.
    /// </summary>
    Character = 1,

    /// <summary>
    /// The type is an extended key on the native platform.
    /// </summary>
    Extended = 2,

    /// <summary>
    /// The type is a system key. i.e. Windows key, scroll lock or caps lock.
    /// </summary>
    System = 3,

    /// <summary>
    /// The type is a modifer key. i.e. ctrl, shift or alt.
    /// </summary>
    Modifier = 4,
}
