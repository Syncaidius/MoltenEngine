using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Molten.Input
{
    public class WindowsClipboard : IClipboard
    {
        public void SetText(string txt)
        {
            RunAsSTAThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(txt))
                    Clipboard.SetText(txt);
            });
        }

        public bool ContainsText()
        {
            bool result = false;
            RunAsSTAThread(() =>
            {
                result = Clipboard.ContainsText();
            });

            return result;
        }

        public string GetText()
        {
            string result = null;

            RunAsSTAThread(() =>
            {
                result = Clipboard.GetText();
            });

            return result;
        }

        /// <summary>Run a callback inside a single-threaded apartment (STA) thread.</summary>
        /// <param name="callback"></param>
        private void RunAsSTAThread(Action callback)
        {
            AutoResetEvent resetter = new AutoResetEvent(false);
            Thread t = new Thread(() =>
            {
                callback();
                resetter.Set();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            resetter.WaitOne();
        }

        public void Dispose()
        {

        }
    }
}
