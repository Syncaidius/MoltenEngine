using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public interface IClipboard
    {
        void SetText(string txt);

        bool ContainsText();

        string GetText();
    }
}
