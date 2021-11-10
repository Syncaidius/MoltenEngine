using System;

namespace Molten.Font
{
    public class FontInterpreterException : Exception
    {
        internal FontInterpreterException(string message) : base(message) { }

        internal FontInterpreterException() : base() { }
    }
}
