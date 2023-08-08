using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Threading;

namespace Molten.Services
{
    public class ServiceStartupProperties
    {
        public EngineService Instance { get; init; }

        public ThreadingMode ThreadMode { get; init; }
    }
}
