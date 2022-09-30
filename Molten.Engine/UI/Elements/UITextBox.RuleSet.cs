using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public partial class UITextBox
    {
        public class RuleSet
        {
            /// <summary>
            /// Gets or sets a list of characters that are considered whitespace
            /// </summary>
            public char[] Whitespace { get; set; } = { ' ', '\t' };

            /// <summary>
            /// Gets or sets a list of characters that are considered punctuation.
            /// </summary>
            public char[] Punctuation { get; set; } = { '.', ',', ':', ';', '\'', '"' };
        }
    }
}
