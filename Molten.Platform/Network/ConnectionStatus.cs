using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net
{
    public enum ConnectionStatus : Byte
    {
        None = 0,

        InitiatedConnect = 1,

        ReceivedInitiation = 2,

        RespondedAwaitingApproval = 3,

        RespondedConnect = 4,

        Connected = 5,

        Disconnecting = 6,

        Disconnected = 7
    }
}