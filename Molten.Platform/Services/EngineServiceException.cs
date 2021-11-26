using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class EngineServiceException : Exception
    {
        public EngineServiceException(EngineService service, string message) : base(message)
        {
            Service = service;
        }

        public EngineService Service { get; }
    }
}
