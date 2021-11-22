using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public struct NetworkMessage
    {
        public byte[] Data { get; }
        public int Sequence { get; }

        public NetworkMessage(byte[] data, int sequence)
        {
            Data = data;
            Sequence = sequence;
        }
    }
}
