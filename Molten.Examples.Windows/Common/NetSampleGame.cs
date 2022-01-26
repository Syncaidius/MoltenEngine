//using Molten.Graphics;
//using Molten.Input;
//using Molten.Net;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Molten.Samples
//{
//    public abstract class NetSampleGame : SampleGame
//    {
//        SceneObject _player; 

//        public NetSampleGame(string title) : base($"Networked " + title) { }


//        protected override void OnStart(EngineSettings settings)
//        {
//            base.OnStart(settings);
//            settings.AddService<LidgrenNetworkService>();
//        }

//        public NetworkService Net => Engine.Net;
//    }
//}
