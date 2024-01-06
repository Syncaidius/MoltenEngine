using Molten.Threading;
using Molten.Utility;
using System.Diagnostics;

namespace Molten;

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
    /// <param name="engine">The parent engine.</param>
    /// <param name="settings"></param>
    /// <param name="mode">The threading mode that the service will use.</param>
    internal void Initialize(Engine engine, EngineSettings settings, ThreadingMode mode)
    {
        Settings = settings;
        Engine = engine;

        try
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Engine.Log.WriteLine($"Initializing service: {GetType()}");

            ThreadMode = mode;

            // Setup service thread, but don't start it yet.
            if (ThreadMode == ThreadingMode.SeparateThread)
                Thread = engine.Threading.CreateThread($"service_{this}", false, false, Update);
            else
                Thread = Engine.MainThread;

            OnInitialize(Settings);

            Log.WriteLine($"Completed initialization of service: {this}");
            State = EngineServiceState.Ready;

            sw.Stop();
            Log.WriteLine($"Initialized service in {sw.Elapsed.TotalMilliseconds:N2} milliseconds");

            OnInitialized?.Invoke(this);
        }
        catch (Exception ex)
        {
            State = EngineServiceState.Error;
            Log.Error($"Failed to initialize service: {this}");
            Log.Error(ex);
        }
    }

    /// <summary>
    /// Requests the current <see cref="EngineService"/> to start.
    /// </summary>
    /// <returns></returns>
    public void Start()
    {
        if (State == EngineServiceState.Uninitialized)
            throw new EngineServiceException(this, "Cannot start uninitialized service.");

        if (State == EngineServiceState.Error)
        {
            Engine.Log.Error($"Cannot start service {this} due to error.");
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

            OnStart(Settings);
            if (Thread != Engine.MainThread)
            {
                Thread.Start();
                Engine.Log.WriteLine($"Started service thread: {Thread.Name}");
            }

            State = EngineServiceState.Running;
            Engine.Log.WriteLine($"Started service: {this}");
            Engine.Log.WriteLine($"Service log: {_logWriter.LogFileInfo.FullName}");

            sw.Stop();
            Log.WriteLine($"Started service in {sw.Elapsed.TotalMilliseconds:N2} milliseconds");

            OnStarted?.Invoke(this);
        }
        catch (Exception ex)
        {
            State = EngineServiceState.Error;
            Engine.Log.Error($"Failed to start service: {this}");
            Engine.Log.Error(ex);
            OnError?.Invoke(this);
        }
    }

    /// <summary>
    /// Stops the current <see cref="EngineService"/>.
    /// </summary>
    public void Stop(bool waitForStop)
    {
        OnStop(Settings);

        if (Thread != Engine.MainThread)
        {
            if (waitForStop)
            {
                Thread?.DisposeAndJoin();

                // Wait for the service thread to stop.
                while (State != EngineServiceState.Ready)
                {
                    if (Thread != null)
                    {
                        if (Thread.IsDisposed)
                            break;

                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            else
            {
                Thread?.Dispose();
            }
        }

        State = EngineServiceState.Ready;
    }

    protected override void OnDispose()
    {
        Stop(true);

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

    /// <summary>
    /// Invoked when the current <see cref="EngineService"/> is being initialized.
    /// </summary>
    /// <param name="settings">The <see cref="EngineSettings"/>.</param>
    /// <returns>The threading mode that the current <see cref="EngineThread"/> should be run in, when <see cref="Start()"/> is called.</returns>
    protected abstract void OnInitialize(EngineSettings settings);

    /// <summary>
    /// Invoked when the current <see cref="EngineService"/> has been requested to start.
    /// <para>For a service running in <see cref="ThreadingMode.SeparateThread"/>, 
    /// <see cref="OnStart(EngineSettings)"/> is invoked after the service thread is created.</para>
    /// </summary>
    /// <param name="settings">The <see cref="EngineSettings"/>.</param>
    protected abstract void OnStart(EngineSettings settings);

    /// <summary>
    /// Invoked when the current <see cref="EngineService"/> has been requested to stop.
    /// </summary>
    /// <param name="settings">The <see cref="EngineSettings"/>.</param>
    protected abstract void OnStop(EngineSettings settings);

    /// <summary>Invoked when the current <see cref="EngineService"/> needs to be updated.</summary>
    /// <param name="time"></param>
    protected abstract void OnUpdate(Timing time);

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
    public ThreadingMode ThreadMode { get; private set; }

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

    /// <summary>
    /// Gets the parent <see cref="Engine"/> instance.
    /// </summary>
    public Engine Engine { get; private set; }
}
