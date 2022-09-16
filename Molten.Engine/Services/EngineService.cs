using Molten.Threading;
using Molten.Utility;
using System.Diagnostics;

namespace Molten
{
    public abstract class EngineService : EngineObject
    {
        public event MoltenEventHandler<EngineService> OnInitialized;

        /// <summary>
        /// Invoked after the current <see cref="EngineService"/> has 
        /// completed a <see cref="Start(ThreadManager, Logger)"/> invocation.
        /// </summary>
        public event MoltenEventHandler<EngineService> OnStarted;

        /// <summary>
        /// Invoked if an initialization or start-up error occurs.
        /// </summary>
        public event MoltenEventHandler<EngineService> OnError;

        LogFileWriter _logWriter;

        /// <summary>
        /// Creates a new instance of <see cref="EngineService"/>.
        /// </summary>
        public EngineService()
        {
            Log = Logger.Get();
            string serviceName = this.GetType().Name;

            _logWriter = new LogFileWriter($"Logs/{serviceName}.log");
            Log.AddOutput(_logWriter);
        }

        /// <summary>
        /// Initializes the current <see cref="EngineService"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="parentLog"></param>
        public void Initialize(EngineSettings settings, Logger parentLog)
        {
            Settings = settings;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                parentLog.WriteLine($"Initializing service: {this.GetType()}");
                OnInitialize(Settings);
                parentLog.WriteLine($"Completed initialization of service: {this}");
                State = EngineServiceState.Ready;

                sw.Stop();
                Log.WriteLine($"Initialized service in {sw.Elapsed.TotalMilliseconds:N2} milliseconds");

                OnInitialized?.Invoke(this);
            }
            catch (Exception ex)
            {
                State = EngineServiceState.Error;
                parentLog.Error($"Failed to initialize service: {this}");
                parentLog.Error(ex);
            }
        }

        /// <summary>
        /// Requests the current <see cref="EngineService"/> to start.
        /// </summary>
        /// <param name="threadManager">The <see cref="ThreadManager"/> provided for startup.</param>
        /// <returns></returns>
        public void Start(ThreadManager threadManager, Logger parentLog)
        {
            if (State == EngineServiceState.Uninitialized)
                throw new EngineServiceException(this, "Cannot start uninitialized service.");

            if (State == EngineServiceState.Error)
            {
                parentLog.Error($"Cannot start service {this} due to error.");
                OnError?.Invoke(this);
                return;
            }

            if (State == EngineServiceState.Starting || State == EngineServiceState.Running)
                return;


            State = EngineServiceState.Starting;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                ThreadMode = OnStart(threadManager);
                if (ThreadMode == ThreadingMode.SeparateThread)
                {
                    Thread = threadManager.CreateThread($"service_{this}", true, false, Update);
                    parentLog.WriteLine($"Started service thread: {Thread.Name}");
                }

                State = EngineServiceState.Running;
                parentLog.WriteLine($"Started service: {this}");
                parentLog.WriteLine($"Service log: {_logWriter.LogFileInfo.FullName}");

                sw.Stop();
                Log.WriteLine($"Started service in {sw.Elapsed.TotalMilliseconds:N2} milliseconds");

                OnStarted?.Invoke(this);
            }
            catch (Exception ex)
            {
                State = EngineServiceState.Error;
                parentLog.Error($"Failed to start service: {this}");
                parentLog.Error(ex);
                OnError?.Invoke(this);
            }
        }

        /// <summary>
        /// Stops the current <see cref="EngineService"/>.
        /// </summary>
        public void Stop()
        {
            OnStop();

            Thread?.Dispose();
            State = EngineServiceState.Ready;
        }

        protected override void OnDispose()
        {
            Thread?.DisposeAndJoin();

            OnServiceDisposing();

            State = EngineServiceState.Disposed;
            Thread = null;
            Log.Dispose();
        }

        /// <summary>
        /// Invoked after a the service has started disposal and stopped it's thread (if any). Always invoked before the <see cref="Log"/> is disposed.
        /// </summary>
        protected virtual void OnServiceDisposing() { }

        /// <summary>
        /// Updates the current <see cref="EngineService"/>.
        /// </summary>
        /// <param name="time"></param>
        public void Update(Timing time)
        {
            // TODO track update time taken.
            OnUpdate(time);
        }

        protected abstract void OnInitialize(EngineSettings settings);

        /// <summary>Invoked when the current <see cref="EngineService"/> needs to be updated.</summary>
        /// <param name="time"></param>
        protected abstract void OnUpdate(Timing time);

        /// <summary>
        /// Invokved when the current <see cref="EngineService"/> has been requested to start.
        /// </summary>
        /// <returns></returns>
        protected abstract ThreadingMode OnStart(ThreadManager threadManager);

        /// <summary>
        /// Invoked when the current <see cref="EngineService"/> has been requested to stop.
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        /// Gets the state of the current <see cref="EngineService"/>.
        /// </summary>
        public EngineServiceState State { get; protected set; }

        /// <summary>
        /// Gets the thread bound to the current <see cref="EngineService"/>. 
        /// This will be null if <see cref="ThreadMode"/> is not set to <see cref="ThreadingMode.SeparateThread"/>.
        /// </summary>
        public EngineThread Thread { get; private set; }

        /// <summary>
        /// Gets the threading mode of the current <see cref="EngineService"/>.
        /// </summary>
        public ThreadingMode ThreadMode { get; protected set; }

        /// <summary>
        /// Gets the current instance of engine settings. This will be the same instance that was passed in when the engine was instantiated.
        /// </summary>
        public EngineSettings Settings { get; private set; }

        /// <summary>
        /// Gets the log bound to the current <see cref="EngineService"/>.
        /// </summary>
        public Logger Log { get; private set; }

        /// <summary>Gets the version of the current service. If not set by the service, all version components will be 0.</summary>
        public Version Version { get; protected set; } = new Version();
    }
}
