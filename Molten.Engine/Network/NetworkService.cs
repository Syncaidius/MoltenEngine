using Molten.Collections;
using Molten.Net.Message;
using Molten.Threading;

namespace Molten.Net
{
    public abstract class NetworkService : EngineService
    {
        protected readonly ThreadedQueue<INetworkMessage> _inbox;
        protected readonly ThreadedQueue<(INetworkMessage, INetworkConnection[])> _outbox;

        public NetworkService()
        {
            _inbox = new ThreadedQueue<INetworkMessage>();
            _outbox = new ThreadedQueue<(INetworkMessage, INetworkConnection[])>();
        }


        #region Public
        public int RecievedMessages => _inbox.Count;

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

        #endregion

        #region Protected

        protected override void OnInitialize(EngineSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            // NOTE access Settings property of base EngineService to get network settings/config.

            return ThreadingMode.SeparateThread;
        }

        protected override void OnStop()
        {
            _outbox.Clear();
        }

        protected override void OnDispose()
        {
            Log.Dispose();
        }

        /// <summary>
        /// Gets the network identifier of the current network service.
        /// </summary>
        public string Identity { get; set; } = "Molten Player";

        #endregion

    }
}
