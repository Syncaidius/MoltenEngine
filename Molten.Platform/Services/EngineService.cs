using Molten.Threading;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class EngineService : EngineObject
    {
        /// <summary>
        /// Invoked after the current <see cref="EngineService"/> has 
        /// completed a <see cref="Start(ThreadManager, Logger)"/> invocation.
        /// </summary>
        public event MoltenEventHandler<EngineService> OnStarted;

        public event MoltenEventHandler<EngineService> OnError;

        public abstract void Initialize(SettingBank settings, Logger log);

        /// <summary>
        /// Requests the current <see cref="EngineService"/> to start.
        /// </summary>
        /// <param name="threadManager">The <see cref="ThreadManager"/> provided for startup.</param>
        /// <returns></returns>
        public void Start(ThreadManager threadManager, Logger log)
        {
            if (State == EngineServiceState.Uninitialized)
                throw new EngineServiceException(this, "Cannot start uninitialized service.");

            if (State == EngineServiceState.Error)
            {
                log.WriteError($"Cannot start service {this} due to error.");
                return;
            }

            if (State == EngineServiceState.Starting || State == EngineServiceState.Running)
                return;

            State = EngineServiceState.Starting;
            try
            {
                ThreadMode = OnStart();
                if (ThreadMode == ThreadingMode.SeparateThread)
                {
                    Thread = threadManager.SpawnThread($"service_{this}", true, false, OnUpdate);
                    log.WriteLine($"Started service thread: {Thread.Name}");
                }

                State = EngineServiceState.Running;
                log.WriteLine($"Started service: {this}");
                OnStarted?.Invoke(this);
            }
            catch (Exception ex)
            {
                State = EngineServiceState.Error;
                log.WriteError($"Failed to start service: {this}");
                log.WriteError(ex);
            }
        }

        public void Stop()
        {
            OnStop();

            Thread.Dispose();
            Thread = null;
        }

        /// <summary>
        /// Invokved when the current <see cref="EngineService"/> has been requested to start.
        /// </summary>
        /// <returns></returns>
        protected abstract ThreadingMode OnStart();

        /// <summary>
        /// Invoked when the current <see cref="EngineService"/> has been requested to stop.
        /// </summary>
        protected abstract void OnStop();


        public void Update(Timing time)
        {
            // TODO track update time taken.
            OnUpdate(time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        protected abstract void OnUpdate(Timing time);

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
    }

    public abstract class EngineService<T> : EngineService
        where T : SettingBank
    {
        public event MoltenEventHandler<EngineService<T>, T> OnInitialized;

        public override sealed void Initialize(SettingBank settings, Logger log)
        {
            Settings = settings as T;
            try
            {
                log.WriteLine($"Initializing service: {this.GetType()}");
                OnInitialize(Settings, log);
                log.WriteLine($"Completed initialization of service: {this}");
                State = EngineServiceState.Initialized;
                OnInitialized.Invoke(this, Settings);
            }
            catch(Exception ex)
            {
                State = EngineServiceState.Error;
                log.WriteError($"Failed to initialize service: {this}");
                log.WriteError(ex);
            }
        }

        protected abstract void OnInitialize(T settings, Logger log);

        public T Settings { get; private set; }
    }
}
