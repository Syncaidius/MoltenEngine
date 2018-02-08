using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public interface IClipboard
    {
        /// <summary>Sets the current clipboard text value.</summary>
        /// <param name="txt">The text to be sent to the clipboard.</param>
        void SetText(string txt);

        /// <summary>Checks if the clipboard contains any text. Returns true if it does.</summary>
        /// <returns>A boolean</returns>
        bool ContainsText();

        /// <summary>Gets the current string value on the clipboard.</summary>
        /// <returns>A string containing the clipboard text.</returns>
        string GetText();
    }
}
