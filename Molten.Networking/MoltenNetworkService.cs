using Molten.Collections;
using Molten.Networking.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public abstract class MoltenNetworkService : IDisposable
    {
        protected readonly ThreadedQueue<INetworkMessage> _inbox;
        protected readonly ThreadedQueue<INetworkMessage> _outbox;
        
        public MoltenNetworkService()
        {
            Log = Logger.Get();
            _inbox = new ThreadedQueue<INetworkMessage>();
            _outbox = new ThreadedQueue<INetworkMessage>();
        }


        #region Public

        // Called by network service thread.
        public void Update(Timing timing)
        {
            OnUpdate(timing);
        }

        public void Dispose()
        {
            OnDispose();
            Log.Dispose();
        }

        /// <summary>
        /// Puts the message into outbox to be sent on next update.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(INetworkMessage message)
        {
            _outbox.Enqueue(message);
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

        public int RecievedMessages => _inbox.Count;

        #endregion

        #region Protected

        protected abstract void OnUpdate(Timing timing);
        protected abstract void OnDispose();
        protected internal Logger Log { get; }

        #endregion

    }
}
