using Molten.Collections;
using Molten.Network.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Network
{
    public abstract class NetworkManager : EngineService<NetworkSettings>
    {
        protected readonly ThreadedQueue<INetworkMessage> _inbox;
        protected readonly ThreadedQueue<(INetworkMessage, INetworkConnection[])> _outbox;
        
        public NetworkManager()
        {
            _inbox = new ThreadedQueue<INetworkMessage>();
            _outbox = new ThreadedQueue<(INetworkMessage, INetworkConnection[])>();
        }


        #region Public
        public int RecievedMessages => _inbox.Count;

        // Called by network service thread.
        public void Update(Timing timing)
        {
            OnUpdate(timing);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            Log.Dispose();
        }

        /// <summary>
        /// Puts the message into outbox to be sent on next update.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients">Collection of connections to send the message to, broadcast if null.</param>
        public void SendMessage(INetworkMessage message, IEnumerable<INetworkConnection> recipients = null)
        {
            _outbox.Enqueue(new ValueTuple<INetworkMessage, INetworkConnection[]>(message, recipients?.ToArray()));
        }

        /// <summary>
        /// Dequeues a message from the inbox if any.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool TryReadMessage(out INetworkMessage message)
        {
            return _inbox.TryDequeue(out message);
        }


        public abstract IEnumerable<INetworkConnection> GetConnections();

        public abstract INetworkConnection Connect(string host, int port, byte[] data = null);

        /// <summary>
        /// Starts a network peer of a given type.
        /// </summary>
        /// <param name="type"></param>
        public abstract void Start(NetApplicationMode type, int port, string identity);

        #endregion

        #region Protected

        protected internal abstract void OnUpdate(Timing timing);

        protected internal Logger Log { get; }

        #endregion

    }
}
