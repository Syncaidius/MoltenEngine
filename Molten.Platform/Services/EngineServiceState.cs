using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum EngineServiceState
    {
        /// <summary>
        /// The service has not been initialized.
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        /// The service is initialized and ready to (re)start.
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// The service is starting.
        /// </summary>
        Starting = 2,

        /// <summary>
        /// The service is running.
        /// </summary>
        Running = 3,

        /// <summary>
        /// The service failed due to an error.
        /// </summary>
        Error = 10,
    }
}
