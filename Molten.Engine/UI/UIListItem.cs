using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIListItem
    {
        internal UIButton Button;
        string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (Button != null)
                    Button.Text.Text = _text;
            }
        }

        public object Tag { get; set; }

        public int Index { get; internal set; } = -1;
    }
}
