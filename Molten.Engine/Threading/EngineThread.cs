using Molten.Collections;

namespace Molten.Threading;

/// <summary>An thread with its own update and timing loop. Can be paused/stopped/continued at any time. Useful for game components which update on a regular 
/// basis (e.g. game world, rendering, logic, etc).</summary>
/// <seealso cref="System.IDisposable" />
public class EngineThread : IDisposable
{
    ThreadedQueue<Action> _dispatchedActions;
    Timing _timing;
    Thread _thread;
    ManualResetEvent _reset;
    bool _shouldExit;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineThread"/> class.
    /// </summary>
    /// <param name="manager">The <see cref="ThreadManager"/> that will own the new instance of <see cref="EngineThread"/>.</param>
    /// <param name="name">The name.</param>
    /// <param name="fixedTimeStep">If true, the engine thread will try to update at a rate matching the value of <see cref="Timing.TargetFrameTime"/>.</param>
    /// <param name="callback">The callback that the thread should run.</param>
    /// <param name="apartmentState">The apartment state of the thread. The default value is multithreaded apartment (MTA).</param>
    /// <param name="startNow">If true, the thread will be started immediately after creation.</param>
    internal EngineThread(ThreadManager manager, string name, bool startNow, bool fixedTimeStep, Action<Timing> callback, ApartmentState apartmentState = ApartmentState.MTA)
    {
        ApartmentState = apartmentState;
        Manager = manager;
        _reset = new ManualResetEvent(true);
        _timing = new Timing(callback);
        _timing.IsFixedTimestep = fixedTimeStep;
        _dispatchedActions = new ThreadedQueue<Action>();
        Name = name;

        if (startNow)
            Start();
    }

    /// <summary>
    /// Dispatches a callback to be executed by the current <see cref="EngineThread"/> on its next update tick.
    /// </summary>
    public void Dispatch(Action callback)
    {
        _dispatchedActions.Enqueue(callback);
    }

    /// <summary>Starts or resumes the thread's update loop.</summary>
    public void Start()
    {
        if (_thread == null)
        {
            _thread = new Thread(() =>
            {
                while (!_shouldExit)
                {
                    while (_dispatchedActions.TryDequeue(out Action action))
                        action();

                    if (!_timing.IsRunning)
                        _reset.WaitOne();

                    _timing.Update();
                }
            });

            _thread.Name = Name;

            try
            {
                _thread.TrySetApartmentState(ApartmentState);
            }
            catch { }
        }
        else
        {
            // TODO Add EngineThreadState and set it to running.
        }

        _thread.Start();
        _timing.Start();
        _reset.Set();
    }

    /// <summary>Pauses the thread's update loop.</summary>
    public void Pause()
    {
        _timing.Pause();
        _reset.Reset();
    }

    /// <summary>Attempts to exit the thread.</summary>
    public void Exit()
    {
        _shouldExit = true;
        _reset.Set();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        Exit();
        Manager.DestroyThread(this);
        _reset.Dispose();
        IsDisposed = true;
    }

    /// <summary>Disposes of the thread after waiting for it to stop itself first.</summary>
    public void DisposeAndJoin()
    {
        if (IsDisposed)
            return;

        Exit();
        if (_thread != Thread.CurrentThread)
            _thread?.Join();

        Manager.DestroyThread(this);
        IsDisposed = true;
    }

    /// <summary>
    /// Gets the name of the thread.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>Gets the timing instance bound to the <see cref="EngineThread"/>. </summary>
    public Timing Timing => _timing;

    /// <summary>Gets whether or not the <see cref="EngineThread"/> has been disposed.</summary>>
    public bool IsDisposed { get; internal set; }

    /// <summary>
    /// Gets the <see cref="System.Threading.ApartmentState"/> that was chosen when the thread was created.
    /// </summary>
    public ApartmentState ApartmentState { get; }

    /// <summary>
    /// Gets the <see cref="ThreadManager"/> which owns the current <see cref="EngineThread"/>.
    /// </summary>
    public ThreadManager Manager { get; }
}
