using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten
{
    public class StringParameters : IContentParameters
    {
        public bool IsBinary { get; set; } = false;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}
