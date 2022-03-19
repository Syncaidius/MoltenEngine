using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentParameter
    {
        internal ContentParameter() { }

        public string Name { get; internal set; }

        public Type ExpectedType { get; internal set; }

        public object DefaultValue { get; internal set; }
    }
}
