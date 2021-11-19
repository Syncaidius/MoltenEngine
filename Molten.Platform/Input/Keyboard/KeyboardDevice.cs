namespace Molten.Input
{
    public delegate void KeyHandler(KeyboardDevice device, KeyCode key);
    public delegate void KeyPressHandler(char character, long paramVal);

    public abstract class KeyboardDevice : InputDevice<KeyboardKeyState, KeyCode>
    {
        public KeyboardDevice(IInputManager manager, Logger log) : 
            base(manager, manager.Settings.KeyboardBufferSize, log)
        {

        }

        /// <summary>
        /// Occurs when a character key is pressed.
        /// </summary>
        public event KeyPressHandler OnCharacterKey;

        /// <summary>
        /// Occurs when any type of key is pressed.
        /// </summary>
        public event KeyHandler OnKeyPressed;

        /// <summary>
        /// Occurs when a previously-pressed key (of any type) is released.
        /// </summary>
        public event KeyHandler OnKeyReleased;
    }
}
