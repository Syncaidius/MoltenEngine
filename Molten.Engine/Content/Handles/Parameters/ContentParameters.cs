using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class ContentParameters : ICloneable
    {
        public int PartCount = 1;

        public abstract object Clone();
    }
}
