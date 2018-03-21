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

        int _blockingVal;

        internal Logger()
        {
            _errorBuilder = new StringBuilder();
            _outputs = new List<ILogOutput>();
            _loggers = new ThreadedList<Logger>();
            _blockingVal = 0;
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

        public void WriteLine(string value, Color color)
        {
            SpinWait spin = new SpinWait();

            while (0 != Interlocked.Exchange(ref _blockingVal, 1))
                spin.SpinOnce();

            for (int i = 0; i < _outputs.Count; i++)
                _outputs[i].WriteLine(value, color);
#if DEBUG
            Debug.WriteLine(value);
#endif
            Interlocked.Exchange(ref _blockingVal, 0);
            return;
        }

        public void Clear()
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    for (int i = 0; i < _outputs.Count; i++)
                        _outputs[i].Clear();

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }

                spin.SpinOnce();
            }
        }

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
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    for (int i = 0; i < _outputs.Count; i++)
                        _outputs[i].Write(value, color);

#if DEBUG
                    Debug.Write(value);
#endif
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }

                spin.SpinOnce();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _outputs.Count; i++)
                _outputs[i].Dispose();
        }

        /// <summary>The default color of errors when calling <see cref="WriteError(Exception, bool)"/> or <see cref="Error(string)"/>.</summary>
        public Color ErrorColor { get; set; } = Color.Red;

        public Color WarningColor { get; set; } = new Color(200, 200, 100, 255);

        public Color DebugColor { get; set; } = new Color(100, 100, 100, 255);

        public string ErrorPrefix { get; set; } = "[ERROR]";

        public string WarningPrefix { get; set; } = "[WARNING]";
    }
}
