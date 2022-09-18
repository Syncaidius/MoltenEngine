using Molten.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Molten
{
    public class Logger : IDisposable
    {
        public class Entry
        {
            public DateTime TimeStamp { get; internal set; }

            public string TimeStampString { get; internal set; }

            public string Text { get; internal set; }

            public LogCategory Category { get; internal set; }

            public Color Color { get; set; } = Color.White;

            public Entry Previous { get; internal set; }

            public Entry Next { get; internal set; }

            public uint LineNumber { get; set; } = 1U;
        }

        List<ILogOutput> _outputs;
        static string[] _traceSeparators = { Environment.NewLine };
        static ThreadedList<Logger> _loggers;

        Entry _first;
        Entry _last;
        uint _entryCount;
        Interlocker _interlocker;

        static Logger()
        {
            _loggers = new ThreadedList<Logger>();
        }

        internal Logger()
        {
            _first = new Entry();
            _last = _first;

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

        public void WriteLine(string value)
        {
            WriteLine(value, Color.White);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        [Conditional("DEBUG")]
        public void Debug(string value)
        {
            WriteLine($"[DEBUG] {value}", Color.White);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="filename">The filename associated with the message.</param>
        [Conditional("DEBUG")]
        public void Debug(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                WriteLine($"[DEBUG] {value}", DebugColor, LogCategory.Debug);
            else
                WriteLine($"[DEBUG] {filename}: {value}", DebugColor, LogCategory.Debug);
        }

        /// <summary>A debug version of <see cref="WriteLine(string)"/> which will be ignored and removed in release builds.</summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="color">The color of the text.</param>
        [Conditional("DEBUG")]
        public void WriteDebugLine(string value, Color color)
        {
            WriteLine($"[DEBUG] {value}", color, LogCategory.Debug);
        }

        /// <summary>
        /// Writes a line of text to all of the attached <see cref="ILogOutput"/> instances.
        /// </summary>
        /// <param name="text">The message to be written to the logger.</param>
        /// <param name="color">The preferred color that the text should be written in.</param>
        /// <param name="category">The category of the logged message.</param>
        public void WriteLine(string text, Color color, LogCategory category = LogCategory.Message)
        {
            WriteInternal(text, color, category);

            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].Write(text, _last);
#if DEBUG
                System.Diagnostics.Debug.WriteLine(text);
#endif

                // Create the next, entry for a new line.
                Entry e = new Entry();
                e.LineNumber = _last.LineNumber + 1;
                _last.Next = e;
                e.Previous = _last;
                _last = e;

                // Cull oldest log entry if required.
                if (_entryCount == MaxEntryCount && _first != _last)
                {
                    _first = _first.Next;
                    _first.Previous = null;
                }
                else
                {
                    _entryCount++;
                }
            });
        }

        public void Write(string value, LogCategory category = LogCategory.Message)
        {
            Write(value, Color.White, category);
        }

        public void Write(string text, Color color, LogCategory category = LogCategory.Message)
        {
            WriteInternal(text, color, category);

            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _outputs.Count; i++)
                    _outputs[i].Write(text, _last);

#if DEBUG
                System.Diagnostics.Debug.Write(text);
#endif
            });
        }

        private void WriteInternal(string text, Color color, LogCategory category)
        {
            _last.Text += text;
            _last.Color = color;
            _last.Category = category;
            _last.TimeStamp = DateTime.Now;
            _last.TimeStampString = _last.TimeStamp.ToLongTimeString();
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

            _first = new Entry();
            _last = _first;
            _entryCount = 0;
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

            if (handled)
                WriteLine("===HANDLED EXCEPTION===", ErrorColor, LogCategory.Error);
            else
                WriteLine("===UNHANDLED EXCEPTION===", ErrorColor, LogCategory.Error);

            WriteLine(title, ErrorColor, LogCategory.Error);
            WriteLine(msg, ErrorColor, LogCategory.Error);
            WriteLine(source, ErrorColor, LogCategory.Error);
            WriteLine(hResult, ErrorColor, LogCategory.Error);
            WriteLine(target, ErrorColor, LogCategory.Error);

            if (e.InnerException != null)
            {
                string inner = "   Inner Exception: " + e.InnerException.GetType().ToString();
                WriteLine(title, ErrorColor, LogCategory.Error);
            }

            // Stack-trace lines.
            for (int i = 0; i < st.Length; i++)
                WriteLine(st[i], ErrorColor, LogCategory.Error);
        }

        public void Error(string value)
        {
            WriteLine(value, ErrorColor, LogCategory.Error);
        }

        public void Error(string value, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                Error(value);
            else
                WriteLine($"{filename}: {value}", ErrorColor, LogCategory.Error);
        }

        public void Warning(string value)
        {
            WriteLine($"{value}", WarningColor, LogCategory.Warning);
        }

        public void Warning(string value, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                Warning(value);
            else
                WriteLine($"{filename}: {value}", WarningColor, LogCategory.Warning);
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
        /// Gets the first <see cref="Entry"/> held by the log.
        /// </summary>
        public Entry FirstEntry => _first;

        /// <summary>
        /// Gets the last <see cref="Entry"/> held by the log.
        /// </summary>
        public Entry LastEntry => _last;

        /// <summary>
        /// Gets the total number of entries logged by the logger so far. <see cref="Clear"/> will reset this value.
        /// </summary>
        public uint EntryCount => _entryCount;

        /// <summary>
        /// Gets or sets the maximum number of <see cref="Entry"/> to maintain in the current <see cref="Logger"/>. 
        /// <para>Once <see cref="EntryCount"/> hits this number, the oldest log entries will be replaced.</para>
        /// </summary>
        public uint MaxEntryCount { get; set; } = 2000;
    }
}
