using Lidgren.Network;
using Molten.Net.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net
{
    internal class LidgrenConnectionStatusChanged : ConnectionStatusChanged
    {
        private NetConnection _connection;

        public LidgrenConnectionStatusChanged(NetIncomingMessage statusChanged)
            : base(GetData(statusChanged.Data, statusChanged.LengthBytes), statusChanged.DeliveryMethod.ToMolten(), statusChanged.SequenceChannel)
        {
            _connection = statusChanged.SenderConnection;
        }

        private static byte[] GetData(byte[] data, int length)
        {
            byte[] dest = new byte[length - 1];
            Array.Copy(data, 1, dest, 0, length - 1);
            return dest;
        }

        public override INetworkConnection Connection => new LidgrenConnection(_connection);
    }
}