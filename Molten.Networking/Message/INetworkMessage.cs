using Molten.Networking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking.Message
{
    public interface INetworkMessage
    {
        byte[] Data { get; }
        int Sequence { get; }
        DeliveryMethod DeliveryMethod { get; }
    }
}
