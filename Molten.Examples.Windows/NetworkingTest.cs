//using Molten.Font;
//using Molten.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Molten.Samples
//{
//    public class SpriteFontStress : SampleSceneGame
//    {
//        private const string NET_IDENTITY = "Molten Network Test";
//        private const int NET_PORT = 2134;


//        public override string Description => "A basic networking test.";

//        Networking.LidgrenNetworkService _client;

//        public SpriteFontStress() 
//            : base("Networking Test")
//        {

//        }

//        protected override void OnInitialize(Engine engine)
//        {
//            base.LoadNetworkingService<Networking.LidgrenNetworkService>();
//            engine.StartNetworkService();

//            engine.NetworkService.Start(Networking.Enums.ServiceType.Server, NET_PORT, NET_IDENTITY);


//            _client = new Networking.LidgrenNetworkService();
//            _client.Start(Networking.Enums.ServiceType.Client, NET_PORT + 1, NET_IDENTITY);
//            _client.Connect(System.Net.IPAddress.Loopback.ToString(), NET_PORT, Encoding.UTF8.GetBytes("Hail!"));

//            _client.SendMessage(new Networking.Message.NetworkMessage(Encoding.UTF8.GetBytes("Message1"), Networking.Enums.DeliveryMethod.Unreliable, 0));
//            _client.SendMessage(new Networking.Message.NetworkMessage(Encoding.UTF8.GetBytes("Message2"), Networking.Enums.DeliveryMethod.Unreliable, 0));
//            _client.SendMessage(new Networking.Message.NetworkMessage(Encoding.UTF8.GetBytes("Message3"), Networking.Enums.DeliveryMethod.Unreliable, 0));

//            base.OnInitialize(engine);
//        }

//        protected override void OnUpdate(Timing time)
//        {

//            while (Engine.NetworkService.TryReadMessage(out var iMessage))
//            {
//                switch (iMessage)
//                {
//                    case Networking.Message.ConnectionRequest connectionRequest:
//                        string hailMessage = Encoding.UTF8.GetString(connectionRequest.Data);
//                        connectionRequest.Approve();
//                        Log.WriteDebugLine("[Server]: Approved connection request: " + hailMessage);
//                        break;

//                    case Networking.Message.NetworkMessage message:
//                        string messageContent = Encoding.UTF8.GetString(message.Data);
//                        Log.WriteDebugLine("[Server]: Recieved message: " + messageContent);
//                        break;

//                    default:
//                        break;
//                }
//            }

//            UpdateClient(time);
//            base.OnUpdate(time);
//        }

//        private void UpdateClient(Timing time)
//        {
//            _client.Update(time);


//            while (_client.TryReadMessage(out var iMessage))
//            {
//                switch (iMessage)
//                {
//                    //case Networking.Message.ConnectionRequest connectionRequest:
//                    //    string hailMessage = Encoding.ASCII.GetString(connectionRequest.Data);
//                    //    Console.WriteLine("[Client] Recieved connection request: " + hailMessage);
//                    //    connectionRequest.Approve();
//                    //    break;

//                    case Networking.Message.NetworkMessage message:
//                        string messageContent = Encoding.ASCII.GetString(message.Data);
//                        Log.WriteDebugLine("[Client]: Recieved message: " + messageContent);
//                        break;


//                    default:
//                        break;
//                }
//            }
//        }
//    }
//}
