using Lidgren.Network;
using Molten.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public class LidgrenNetworkManager : NetworkManager
    {
        NetConnection _connection;


        public LidgrenNetworkManager(NetConnection lidgrenConnection)
        {
            _connection = lidgrenConnection;
        }

        internal NetConnection Connection => _connection;

        public string Host => _connection.RemoteEndPoint.Address.ToString();
        public int Port => _connection.RemoteEndPoint.Port;

        public override IEnumerable<INetworkConnection> GetConnections()
        {
            throw new NotImplementedException();
        }

        public override INetworkConnection Connect(string host, int port, byte[] data = null)
        {
            throw new NotImplementedException();
        }

        public override void Start(NetApplicationMode type, int port, string identity)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(Timing timing)
        {
            throw new NotImplementedException();
        }

        protected override void OnInitialize(NetworkSettings settings, Logger log)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }
    }
}
