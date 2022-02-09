using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Molten.Threading
{
    public enum ThreadingMode
    {
        /// <summary>
        /// Run on the main application <see cref="Thread"/>.
        /// </summary>
        MainThread = 0,

        /// <summary>
        /// Should run on it's own <see cref="EngineThread"/>.
        /// </summary>
        SeparateThread = 1,
    }
}
