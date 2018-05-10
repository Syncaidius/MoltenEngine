using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal delegate void ContentWatcherHandler(string filePath);

    internal class ContentWatcher : EngineObject
    {
        internal ContentWatcher()
        {

        }
    }
}
