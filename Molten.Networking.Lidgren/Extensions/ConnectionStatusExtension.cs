using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net
{
    internal static class ConnectionStatusExtension
    {
        /// <summary>
        /// Convert lidgren delivery method to molten equivalent.
        /// </summary>
        /// <param name="deliveryMethod"></param>
        /// <returns></returns>
        internal static ConnectionStatus ToMolten(this NetConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case NetConnectionStatus.InitiatedConnect:
                    return ConnectionStatus.InitiatedConnect;

                case NetConnectionStatus.ReceivedInitiation:
                    return ConnectionStatus.ReceivedInitiation;

                case NetConnectionStatus.RespondedAwaitingApproval:
                    return ConnectionStatus.RespondedAwaitingApproval;

                case NetConnectionStatus.RespondedConnect:
                    return ConnectionStatus.RespondedConnect;

                case NetConnectionStatus.Connected:
                    return ConnectionStatus.Connected;

                case NetConnectionStatus.Disconnecting:
                    return ConnectionStatus.Disconnecting;

                case NetConnectionStatus.Disconnected:
                    return ConnectionStatus.Disconnected;

                case NetConnectionStatus.None:
                default:
                    return ConnectionStatus.None;
            }
        }

        /// <summary>
        /// Convert molten delivery method to lidgren equivalent.
        /// </summary>
        /// <param name="deliveryMethod"></param>
        /// <returns></returns>
        internal static NetConnectionStatus ToLidgren(this ConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case ConnectionStatus.InitiatedConnect:
                    return NetConnectionStatus.InitiatedConnect;

                case ConnectionStatus.ReceivedInitiation:
                    return NetConnectionStatus.ReceivedInitiation;

                case ConnectionStatus.RespondedAwaitingApproval:
                    return NetConnectionStatus.RespondedAwaitingApproval;

                case ConnectionStatus.RespondedConnect:
                    return NetConnectionStatus.RespondedConnect;

                case ConnectionStatus.Connected:
                    return NetConnectionStatus.Connected;

                case ConnectionStatus.Disconnecting:
                    return NetConnectionStatus.Disconnecting;

                case ConnectionStatus.Disconnected:
                    return NetConnectionStatus.Disconnected;

                case ConnectionStatus.None:
                default:
                    return NetConnectionStatus.None;
            }
        }

    }
}