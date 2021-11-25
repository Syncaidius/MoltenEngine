using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public interface INetworkConnection
    {
        string Host { get; }
        int Port { get; }
    }
}
