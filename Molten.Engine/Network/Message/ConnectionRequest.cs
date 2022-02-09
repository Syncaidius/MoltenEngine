using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net.Message
{
    public abstract class ConnectionRequest : INetworkMessage
    {
        public byte[] Data { get; }
        public int Sequence { get; }
        public DeliveryMethod DeliveryMethod { get; }

        protected ConnectionRequest(byte[] data, DeliveryMethod deliveryMethod, int sequence)
        {
            Data = data;
            Sequence = sequence;
            DeliveryMethod = deliveryMethod;
        }

        public abstract INetworkConnection Connection { get; }
        public abstract void Approve();
        public abstract void Reject(string reason);
    }
}