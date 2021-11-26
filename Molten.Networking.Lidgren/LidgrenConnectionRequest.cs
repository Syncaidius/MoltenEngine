using Lidgren.Network;
using Molten.Net.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net
{
    public class LidgrenConnectionRequest : ConnectionRequest
    {
        private NetConnection _connection;

        public LidgrenConnectionRequest(NetIncomingMessage requestMessage)
            : base(requestMessage.Data, requestMessage.DeliveryMethod.ToMolten(), requestMessage.SequenceChannel)
        {
            _connection = requestMessage.SenderConnection;
        }

        public override INetworkConnection Connection => new LidgrenConnection(_connection);

        public override void Approve()
        {
            _connection.Approve();
            _connection = null;
        }

        public override void Reject(string reason = null)
        {
            _connection.Deny(reason);
            _connection = null;
        }
    }
}