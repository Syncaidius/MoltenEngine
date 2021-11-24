using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public class LidgrenConnection : INetworkConnection
    {
        NetConnection _connection;


        public LidgrenConnection(NetConnection lidgrenConnection)
        {
            _connection = lidgrenConnection;
        }

        internal NetConnection Connection => _connection;

        public string Host => _connection.RemoteEndPoint.Address.ToString();
        public int Port => _connection.RemoteEndPoint.Port;
    }
}
