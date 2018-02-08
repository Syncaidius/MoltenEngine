using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Molten.IO
{
    public class WindowsClipboard : IClipboard
    {
        public void SetText(string txt)
        {
            if (!string.IsNullOrWhiteSpace(txt))
                Clipboard.SetText(txt);
        }

        public bool ContainsText()
        {
            return Clipboard.ContainsText();
        }

        public string GetText()
        {
            string result = result = Clipboard.GetText();

            return result;
        }
    }
}
