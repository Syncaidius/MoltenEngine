using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class NativeObjectVK<T> : EngineObject
        where T : unmanaged
    {
        internal T Native { get; private protected set; }
    }
}
