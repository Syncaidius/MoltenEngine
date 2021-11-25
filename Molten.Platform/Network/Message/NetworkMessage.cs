using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Network.Message
{
    public struct NetworkMessage : INetworkMessage
    {
        public byte[] Data { get; internal set; }
        public int Sequence { get; internal set; }
        public DeliveryMethod DeliveryMethod { get; }

        public NetworkMessage(byte[] data, DeliveryMethod deliveryMethod, int sequence)
        {
            Data = data;
            Sequence = sequence;
            DeliveryMethod = deliveryMethod;
        }
    }
}
