using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class AdapterException : Exception
    {
        public AdapterException(IDisplayAdapter adapter, string message) : base(message)
        {
            Adapter = adapter;
        }

        public IDisplayAdapter Adapter { get; private set; }
    }
}
