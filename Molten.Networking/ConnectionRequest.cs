using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public abstract class ConnectionRequest : INetworkMessage
    {
        public byte[] Data { get; }
        public int Sequence { get; }

        protected ConnectionRequest(byte[] data, int sequence)
        {
            Data = data;
            Sequence = sequence;
        }

        public abstract void Approve();
        public abstract void Reject(string reason);
        public abstract void Recycle();
    }
}
