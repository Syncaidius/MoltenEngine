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
        private ThreadedQueue<NetworkMessage> _recycleBin;

        public ThreadedQueue<NetworkMessage> Inbox { get; }
        public ThreadedQueue<NetworkMessage> Outbox { get; }

        public MoltenNetworkService()
        {
            _recycleBin = new ThreadedQueue<NetworkMessage>();
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
            Log.Dispose();
        }

        public void SendMessage(byte[] data, int sequence)
        {
            Outbox.Enqueue(CreateMessage(data, sequence));
        }








        protected NetworkMessage CreateMessage(byte[] data, int sequence)
        {
            if (_recycleBin.TryDequeue(out NetworkMessage message))
            {
                message.Update(data, sequence);
                return message;
            }

            return new NetworkMessage(data, sequence);
        }


        internal void RecycleMessage(NetworkMessage message)
        {
            message.Clear();

            // Recycle if not a specialized message.
            if (message.GetType() == typeof(NetworkMessage))
                _recycleBin.Enqueue(message);
        }

        protected abstract void OnUpdate(Timing timing);

        protected abstract void OnDispose();

        protected internal Logger Log { get; }
    }
}
