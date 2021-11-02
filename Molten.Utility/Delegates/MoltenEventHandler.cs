using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Utility
{
    public delegate void MoltenEventHandler<T>(T o);

    public delegate void MoltenEventHandler<T, U>(T o1, U o2);
}
