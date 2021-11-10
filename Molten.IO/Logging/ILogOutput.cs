using System;

namespace Molten
{
    public interface ILogOutput : IDisposable
    {
        /// <summary>Writes the specified text to the log output and terminates it with a new line..</summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        void WriteLine(string text, Color color);

        /// <summary>Writes the specified text to the log output.</summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        void Write(string text, Color color);

        /// <summary>Clears the log output.</summary>
        void Clear();
    }
}
