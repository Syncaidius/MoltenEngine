namespace Molten.Input
{
    public delegate void KeyHandler(IKeyboardDevice device, Key key);
    public delegate void KeyPressHandler(char character, long paramVal);

    public interface IKeyboardDevice : IInputDevice<Key>
    {
        /// <summary>
        /// Occurs when a character key is pressed.
        /// </summary>
        event KeyPressHandler OnCharacterKey;

        /// <summary>
        /// Occurs when any type of key is pressed.
        /// </summary>
        event KeyHandler OnKeyPressed;

        /// <summary>
        /// Occurs when a previously-pressed key (of any type) is released.
        /// </summary>
        event KeyHandler OnKeyReleased;
    }
}
