using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Threading
{
    /// <summary>An thread with its own update and timing loop. Can be paused/stopped/continued at any time. Useful for game components which update on a regular 
    /// basis (e.g. game world, rendering, logic, etc).</summary>
    /// <seealso cref="System.IDisposable" />
    public class EngineThread : IDisposable
    {
        ThreadManager _manager;
        Action<EngineThread, Timing> _callback;
        ThreadedQueue<Action> _dispatchedActions;
        Timing _timing;
        Thread _thread;
        ApartmentState _apartmentState;
        bool _shouldExit;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineThread"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="name">The name.</param>
        /// <param name="fixedTimeStep">If true, the engine thread will try to update at a rate matching the value of <see cref="Timing.TargetFrameTime"/>.</param>
        /// <param name="callback">The callback that the thread should run.</param>
        /// <param name="apartmentState">The apartment state of the thread. The default value is multithreaded apartment (MTA).</param>
        /// <param name="startImmediately">If true, the thread will be started immediately after creation.</param>
        internal EngineThread(ThreadManager manager, string name, bool startImmediately, bool fixedTimeStep, Action<Timing> callback, ApartmentState apartmentState = ApartmentState.MTA)
        {
            _apartmentState = ApartmentState;
            _manager = manager;
            _timing = new Timing(callback);
            _timing.IsFixedTimestep = fixedTimeStep;
            _dispatchedActions = new ThreadedQueue<Action>();

            _thread = new Thread(() =>
            {
                while (!_shouldExit)
                {
                    while (_dispatchedActions.TryDequeue(out Action action))
                        action();

                    _timing.Update();
                }
            })
            {
                Name = name,
            };
            _thread.SetApartmentState(apartmentState);
            _thread.Start();

            if (startImmediately)
                _timing.Start();

            Name = name;
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
            _timing.Start();
        }

        /// <summary>Pauses the thread's update loop.</summary>
        public void Pause()
        {
            _timing.Pause();
        }

        /// <summary>Attempts to exit the thread.</summary>
        public void Exit()
        {
            _shouldExit = true;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            _shouldExit = true;
            _manager.DestroyThread(this);
            IsDisposed = true;
        }

        /// <summary>Disposes of the thread after waiting for it to stop itself first.</summary>
        public void DisposeAndJoin()
        {
            if (IsDisposed)
                return;

            _shouldExit = true;
            if (_thread != Thread.CurrentThread)
                _thread.Join();

            _manager.DestroyThread(this);
            IsDisposed = true;
        }

        public string Name { get; private set; }

        /// <summary>Gets the timing instance bound to the <see cref="EngineThread"/>. </summary>
        public Timing Timing => _timing;

        /// <summary>Gets whether or not the <see cref="EngineThread"/> has been disposed.</summary>>
        public bool IsDisposed { get; internal set; }

        /// <summary>
        /// Gets the <see cref="System.Threading.ApartmentState"/> that was chosen when the thread was created.
        /// </summary>
        public ApartmentState ApartmentState => _apartmentState;
    }
}
