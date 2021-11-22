using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public abstract class MoltenNetworkService : IDisposable
    {
        public ThreadedQueue<NetworkMessage> Inbox { get; }
        public ThreadedQueue<NetworkMessage> Outbox { get; }

        public MoltenNetworkService()
        {
            Inbox = new ThreadedQueue<NetworkMessage>();
            Outbox = new ThreadedQueue<NetworkMessage>();
            Log = Logger.Get();
        }

        public void Update(Timing timing)
        {
            OnUpdate(timing);
        }

        public void Dispose()
        {
            OnDispose();
        }



        protected abstract void OnUpdate(Timing timing);

        protected abstract void OnDispose();

        protected internal Logger Log { get; }
    }
}
