using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public interface INetworkMessage
    {
        byte[] Data { get; }
        int Sequence { get; }

        void Recycle();
    }
}
