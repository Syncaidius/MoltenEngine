using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Molten.Networking.Enums;
using Molten.Networking.Message;

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


        public override void Start(ServiceType type)
        {
            if (type == ServiceType.Server)
                _peer = new NetServer(_configuration);
            else
                _peer = new NetClient(_configuration);
            
            _peer.Start();
            Log.WriteLine($"Started network ${Enum.GetName(typeof(ServiceType), type)} on port {_peer.Port}.");
        }

        public override void Connect(string host, int port, byte[] data)
        {
            if (data != null)
            {
                NetOutgoingMessage sendMsg = _peer.CreateMessage();
                sendMsg.Write(data);
                _peer.Connect(host, port, sendMsg);
            }
            else
                _peer.Connect(host, port);
            Log.WriteLine($"Connecting to {host}:{port}.");
        }

        protected override void OnUpdate(Timing timing)
        {
            if (_peer != null)
            {
                ReadMessages();

                if (_peer.Connections.Count > 0)
                    SendMessages();
            }
        }

        private void SendMessages()
        {
            while (_outbox.TryDequeue(out INetworkMessage message))
            {
                NetOutgoingMessage sendMsg = _peer.CreateMessage();
                sendMsg.Write(message.Data);

                _peer.SendMessage(sendMsg, _peer.Connections, message.DeliveryMethod.ToLidgren(), message.Sequence);
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

                    case NetIncomingMessageType.ConnectionApproval:
                        _inbox.Enqueue(new LidgrenConnectionRequest(msg));
                        break;

                    case NetIncomingMessageType.Data:
                        _inbox.Enqueue(new NetworkMessage(msg.Data, msg.DeliveryMethod.ToMolten(), msg.SequenceChannel));
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
