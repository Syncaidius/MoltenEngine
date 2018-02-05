using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum GameRunState
    {
        Started = 0,

        Paused = 1,

        Running = 2,

        Exiting = 4,

        Exited = 5,
    }
}
