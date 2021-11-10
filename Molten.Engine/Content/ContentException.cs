using System;

namespace Molten
{
    public class ContentException : Exception
    {
        internal ContentException(string message) : base(message) { }
    }
}
