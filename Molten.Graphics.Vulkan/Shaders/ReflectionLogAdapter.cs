using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpirvReflector;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Allows a <see cref="SpirvReflection"/> instance to write to a <see cref="Logger"/> instance by implementing <see cref="IReflectionLogger"/>.
    /// </summary>
    internal class ReflectionLogAdapter : IReflectionLogger
    {
        Logger _log;

        internal ReflectionLogAdapter(Logger log)
        {
            _log = log;
        }

        public void Error(string text)
        {
            _log.Error(text);
        }

        public void Warning(string text)
        {
            _log.Warning(text);
        }

        public void Write(string value, ConsoleColor color = ConsoleColor.White)
        {
            Color wColor = ConvertConsoleColor(color);

            _log.Write(value, wColor, LogCategory.Message);
        }

        public void WriteLabeled(string label, string text, ConsoleColor labelColor = ConsoleColor.DarkGray, ConsoleColor textColor = ConsoleColor.White)
        {
            Write(label, labelColor);
            Write(text, textColor);
        }

        public void WriteLine(string value, ConsoleColor color = ConsoleColor.White)
        {
            Color wColor = ConvertConsoleColor(color);
            _log.WriteLine(value, wColor);
        }


        private Color ConvertConsoleColor(ConsoleColor color)
        {
            // Convert ConsoleColor to Molten.Color.
            switch (color)
            {
                default:
                case ConsoleColor.White:
                    return Color.White;

                case ConsoleColor.Black: return Color.Black;
                case ConsoleColor.Blue: return Color.Blue;
                case ConsoleColor.Cyan: return Color.Cyan;
                case ConsoleColor.DarkBlue: return Color.DarkBlue;
                case ConsoleColor.DarkCyan: return Color.DarkCyan;
                case ConsoleColor.DarkGray: return Color.DarkGray;
                case ConsoleColor.DarkGreen: return Color.DarkGreen;
                case ConsoleColor.DarkMagenta: return Color.DarkMagenta;
                case ConsoleColor.DarkRed: return Color.DarkRed;
                case ConsoleColor.DarkYellow: return new Color(0.3f, 0.3f, 0f, 255);
                case ConsoleColor.Gray: return Color.Gray;
                case ConsoleColor.Green: return Color.Green;
                case ConsoleColor.Magenta: return Color.Magenta;
                case ConsoleColor.Red: return Color.Red;
                case ConsoleColor.Yellow: return Color.Yellow;
            }
        }
    }
}
