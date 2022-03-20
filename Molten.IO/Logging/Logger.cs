using Molten.Collections;
using System.Diagnostics;
using System.Text;

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
            _loggers.For(0, 1, (index, log) => log.Dispose());
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

        public void Log(string value)
        {
            Log(value, Color.White);
        }

        /// <summary>A debug version of <see cref="Log(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        [Conditional("DEBUG")]
        public void Debug(string value)
        {
            Log($"[DEBUG] {value}", Color.White);
        }

        /// <summary>A debug version of <see cref="Log(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="filename">The filename associated with the message.</param>
        [Conditional("DEBUG")]
        public void Debug(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                Log($"[DEBUG] {value}", DebugColor);
            else
                Log($"[DEBUG] {filename}: {value}", DebugColor);
        }

        /// <summary>A debug version of <see cref="Log(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="color">The color of the text.</param>
        [Conditional("DEBUG")]
        public void WriteDebugLine(string value, Color color)
        {
            Log($"[DEBUG] {value}", color);
        }

        /// <summary>
        /// Writes a line of text to all of the attached <see cref="ILogOutput"/> instances.
        /// </summary>
        /// <param name="msg">The message to be written to the logger.</param>
        /// <param name="color">The preferred color that the text should be written in.</param>
        public void Log(string msg, Color color)
        {
            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].WriteLine(msg, color);
#if DEBUG
                System.Diagnostics.Debug.WriteLine(msg);
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
        public void Error(Exception e, bool handled = false)
        {
            string[] st = e.StackTrace.Split(_traceSeparators, StringSplitOptions.RemoveEmptyEntries);

            string title = "   Type: " + e.GetType().ToString();
            string msg = "   Message: " + e.Message;
            string source = "   Source: " + e.Source;
            string hResult = "   HResult: " + e.HResult;
            string target = "   Target Site: " + e.TargetSite.Name;

            string time = DateTime.Now.ToLongTimeString();
            if (handled)
                Log("===HANDLED EXCEPTION===", ErrorColor);
            else
                Log("===UNHANDLED EXCEPTION===", ErrorColor);

            Log(title, ErrorColor);
            Log(msg, ErrorColor);
            Log(source, ErrorColor);
            Log(hResult, ErrorColor);
            Log(target, ErrorColor);

            if (e.InnerException != null)
            {
                string inner = "   Inner Exception: " + e.InnerException.GetType().ToString();
                Log(title, ErrorColor);
            }

            // Stack-trace lines.
            for (int i = 0; i < st.Length; i++)
                Log(st[i], ErrorColor);
        }

        public void Error(string value)
        {
            Log($"{ErrorPrefix} {value}", ErrorColor);
        }

        public void Error(string value, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                Log($"{ErrorPrefix}: {value}", ErrorColor);
            else
                Log($"{ErrorPrefix} {filename}: {value}", ErrorColor);
        }

        public void Warning(string value)
        {
            Log($"{WarningPrefix} {value}", WarningColor);
        }

        public void Warning(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                Warning(value);
            else
                Log($"{WarningPrefix} {filename}: {value}", WarningColor);
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
                System.Diagnostics.Debug.Write(value);
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
        /// Gets or sets he default color of errors when calling <see cref="Error(Exception, bool)"/> or <see cref="Error(string)"/>.
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
