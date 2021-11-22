using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    internal class LidgrenConnectionRequest : ConnectionRequest
    {
        private NetConnection _connection;

        public LidgrenConnectionRequest(NetIncomingMessage requestMessage) 
            : base(requestMessage.Data, requestMessage.SequenceChannel)
        {
            _connection = requestMessage.SenderConnection;
        }

        public override void Approve()
        {
            _connection.Approve();
        }

        public override void Reject(string reason = null)
        {
            _connection.Deny(reason);
        }

        public override void Recycle()
        {
            _connection = null;
        }
    }
}
