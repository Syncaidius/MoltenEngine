namespace Molten.Input
{
    public struct KeyboardKeyState : IInputState
    {
        public KeyCode Key;

        public KeyboardKeyType KeyType;

        public DateTime PressTimestamp { get; set; }

        public InputAction Action { get; set; }

        public int SetID { get; set; }

        public ulong UpdateID { get; set; }

        /// <summary>Gets the character value of the key. 
        /// This is only populated if <see cref="KeyType"/> is equal to <see cref="KeyboardKeyType.Character"/></summary>
        public char Character;
    }
}
