using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten
{
    public class Logger : IDisposable
    {
        List<ILogOutput> _outputs;
        StringBuilder _errorBuilder;
        static string[] _traceSeparators = { Environment.NewLine };
        static ThreadedList<Logger> _loggers;

        Interlocker _interlocker;

        static Logger()
        {
            _loggers = new ThreadedList<Logger>();
        }

        internal Logger()
        {
            _errorBuilder = new StringBuilder();
            _outputs = new List<ILogOutput>();
            _interlocker = new Interlocker();
        }

        /// <summary>Gets a new instance of <see cref="Logger"/>. All logs will be closed if <see cref="DisposeAll"/> is called.</summary>
        public static Logger Get()
        {
            Logger log = new Logger();
            _loggers.Add(log);
            return log;
        }

        public static void DisposeAll()
        {
            _loggers.ForInterlock(0, 1, (index, item) =>
            {
                item.Dispose();
                return false;
            });

            _loggers.Clear();
        }

        /// <summary>Adds a <see cref="TextWriter"/> to which the logger's output will be written.</summary>
        /// <param name="writer"></param>
        public void AddOutput(ILogOutput writer)
        {
            if (!_outputs.Contains(writer))
                _outputs.Add(writer);
        }

        public void RemoveOutput(ILogOutput output)
        {
            _outputs.Remove(output);
        }

        public void WriteLine(string value)
        {
            WriteLine(value, Color.White);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        [Conditional("DEBUG")]
        public void WriteDebugLine(string value)
        {
            WriteLine($"[DEBUG] {value}", Color.White);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="filename">The filename associated with the message.</param>
        [Conditional("DEBUG")]
        public void WriteDebugLine(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                WriteLine($"[DEBUG] {value}", DebugColor);
            else
                WriteLine($"[DEBUG] {filename}: {value}", DebugColor);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="color">The color of the text.</param>
        [Conditional("DEBUG")]
        public void WriteDebugLine(string value, Color color)
        {
            WriteLine($"[DEBUG] {value}", color);
        }

        /// <summary>
        /// Writes a line of text to all of the attached <see cref="ILogOutput"/> instances.
        /// </summary>
        /// <param name="msg">The message to be written to the logger.</param>
        /// <param name="color">The preferred color that the text should be written in.</param>
        public void WriteLine(string msg, Color color)
        {
            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].WriteLine(msg, color);
#if DEBUG
                Debug.WriteLine(msg);
#endif
            });
        }

        /// <summary>
        /// Clears the logger and all of it's attached <see cref="ILogOutput"/> instances.
        /// </summary>
        public void Clear()
        {
            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].Clear();
            });
        }

        /// <summary>
        /// Writes an exception (and it's stack-trace) to the logger and all of it's attached <see cref="ILogOutput"/> instances.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="handled">If true, the exception will be marked as handled in it's log message.</param>
        public void WriteError(Exception e, bool handled = false)
        {
            string[] st = e.StackTrace.Split(_traceSeparators, StringSplitOptions.RemoveEmptyEntries);

            string title = "   Type: " + e.GetType().ToString();
            string msg = "   Message: " + e.Message;
            string source = "   Source: " + e.Source;
            string hResult = "   HResult: " + e.HResult;
            string target = "   Target Site: " + e.TargetSite.Name;

            string time = DateTime.Now.ToLongTimeString();
            if (handled)
                WriteLine("===HANDLED EXCEPTION===", ErrorColor);
            else
                WriteLine("===UNHANDLED EXCEPTION===", ErrorColor);

            WriteLine(title, ErrorColor);
            WriteLine(msg, ErrorColor);
            WriteLine(source, ErrorColor);
            WriteLine(hResult, ErrorColor);
            WriteLine(target, ErrorColor);

            if (e.InnerException != null)
            {
                string inner = "   Inner Exception: " + e.InnerException.GetType().ToString();
                WriteLine(title, ErrorColor);
            }

            // Stack-trace lines.
            for (int i = 0; i < st.Length; i++)
                WriteLine(st[i], ErrorColor);
        }

        public void WriteError(string value)
        {
            WriteLine($"{ErrorPrefix} {value}", ErrorColor);
        }

        public void WriteError(string value, string filename)
        {
            if(string.IsNullOrWhiteSpace(filename))
                WriteLine($"{ErrorPrefix}: {value}", ErrorColor);
            else
                WriteLine($"{ErrorPrefix} {filename}: {value}", ErrorColor);
        }

        public void WriteWarning(string value)
        {
            WriteLine($"{WarningPrefix} {value}", WarningColor);
        }

        public void WriteWarning(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                WriteWarning(value);
            else
                WriteLine($"{WarningPrefix} {filename}: {value}", WarningColor);
        }

        public void Write(string value)
        {
            Write(value, Color.White);
        }

        public void Write(string value, Color color)
        {
            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].Write(value, color);

#if DEBUG
                Debug.Write(value);
#endif
            });
        }

        /// <summary>
        /// Disposes the logger and all of it's attached <see cref="ILogOutput"/>.
        /// </summary>
        public void Dispose()
        {
            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].Dispose();
            });
        }

        /// <summary>
        /// Gets or sets he default color of errors when calling <see cref="WriteError(Exception, bool)"/> or <see cref="WriteError(string)"/>.
        /// </summary>
        public Color ErrorColor { get; set; } = Color.Red;

        /// <summary>
        /// Gets or sets the color for warning messages.
        /// </summary>
        public Color WarningColor { get; set; } = new Color(200, 200, 100, 255);

        /// <summary>
        /// Gets or sets the color for debug messages.
        /// </summary>
        public Color DebugColor { get; set; } = new Color(100, 100, 100, 255);

        /// <summary>
        /// Gets or sets the prefix for errors messages.
        /// </summary>
        public string ErrorPrefix { get; set; } = "[ERROR]";

        /// <summary>
        /// Gets or sets the prix for warning messages.
        /// </summary>
        public string WarningPrefix { get; set; } = "[WARNING]";
    }
}
