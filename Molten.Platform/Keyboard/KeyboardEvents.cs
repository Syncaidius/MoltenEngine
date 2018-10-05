using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Input
{
    public delegate void KeyPressHandler(CharacterEventArgs e);

    public class CharacterEventArgs : EventArgs
    {
        private readonly char character;
        private readonly long lParam;

        /// <summary>Creates a new instance of <see cref="CharacterEventArgs"/>.</summary>
        /// <param name="character"></param>
        /// <param name="lParam"></param>
        public CharacterEventArgs(char character, long lParam)
        {
            this.character = character;
            this.lParam = lParam;
        }

        /// <summary>Gets the character that the event represents.</summary>
        public char Character
        {
            get { return character; }
        }

        /// <summary>Gets the raw window message loop parameter value.</summary>
        public long Param
        {
            get { return lParam; }
        }

        /// <summary>Gets the number of times the character has been repeated since the last update.</summary>
        public long RepeatCount
        {
            get { return lParam & 0xffff; }
        }

        /// <summary>Gets whether or not the character or key is from an extended set.</summary>
        public bool ExtendedKey
        {
            get { return (lParam & (1 << 24)) > 0; }
        }

        /// <summary>Gets whether or not ALT is held down while the character key is being pressed..</summary>
        public bool AltPressed
        {
            get { return (lParam & (1 << 29)) > 0; }
        }

        /// <summary>Gets the previous state of the character key (pressed or unpressed).</summary>
        public bool PreviousState
        {
            get { return (lParam & (1 << 30)) > 0; }
        }

        /// <summary>Gets the transition state of the key. True if being released. False if being pressed.</summary>
        public bool TransitionState
        {
            get { return (lParam & (1 << 31)) > 0; }
        }
    }
}
