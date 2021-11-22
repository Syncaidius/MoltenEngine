using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Molten.Networking
{
    internal class LidgrenNetworkService : Networking.MoltenNetworkService
    {
        NetPeerConfiguration _configuration;

        NetPeer _peer;

        public LidgrenNetworkService(int port, string identity)
        {
            _configuration = new NetPeerConfiguration(identity);
            _configuration.Port = port;
        }

        public void InitializeServer()
        {
            _peer = new NetServer(_configuration);
            _peer.Start();
        }


        protected override void OnUpdate(Timing timing)
        {
            ReadMessages();
            SendMessages();
        }

        private void SendMessages()
        {
            while (Outbox.TryDequeue(out NetworkMessage message))
            {
                NetOutgoingMessage sendMsg = _peer.CreateMessage();
                sendMsg.Write(message.Data);
                _peer.SendMessage(sendMsg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, message.Sequence);
            }
        }

        public void ReadMessages()
        {
            NetIncomingMessage msg;
            while ((msg = _peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                        Log.WriteDebugLine(msg.ReadString());
                        break;

                    case NetIncomingMessageType.WarningMessage:
                        Log.WriteWarning(msg.ReadString());
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        Log.WriteError(msg.ReadString());
                        break;


                    case NetIncomingMessageType.Data:
                        NetworkMessage message = new NetworkMessage(msg.Data, msg.SequenceChannel);
                        Inbox.Enqueue(message);
                        break;

                    default:
                        Log.WriteError("Unhandled message type: " + msg.MessageType);
                        break;
                }
                _peer.Recycle(msg);
            }
        }

        protected override void OnDispose()
        {
            foreach (NetConnection connection in _peer.Connections)
                connection.Disconnect("Client shudown.");
        }
    }
}
