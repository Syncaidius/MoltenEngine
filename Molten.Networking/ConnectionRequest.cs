using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public abstract class ConnectionRequest : NetworkMessage
    {
        protected ConnectionRequest(byte[] data, int sequence) 
            : base(data, sequence)
        {

        }

        public abstract void Approve();
        public abstract void Reject(string reason);
    }
}
