namespace Molten.Input
{
    public delegate void KeyHandler(KeyboardDevice device, Key key);
    public delegate void KeyPressHandler(char character, long paramVal);

    public abstract class KeyboardDevice : InputDevice<Key>
    {
        public KeyboardDevice(IInputManager manager, Logger log) : base(manager, log)
        {

        }

        /// <summary>
        /// Occurs when a character key is pressed.
        /// </summary>
        public abstract event KeyPressHandler OnCharacterKey;

        /// <summary>
        /// Occurs when any type of key is pressed.
        /// </summary>
        public abstract event KeyHandler OnKeyPressed;

        /// <summary>
        /// Occurs when a previously-pressed key (of any type) is released.
        /// </summary>
        public abstract event KeyHandler OnKeyReleased;
    }
}
