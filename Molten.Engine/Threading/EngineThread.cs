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
        Timing _timing;
        Thread _thread;
        bool _shouldExit;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineThread"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="name">The name.</param>
        /// <param name="fixedTimestep">If true, the engine thread will try to update at a rate matching the value of <see cref="Timing.TargetFrameTime"/>.</param>
        /// <param name="callback">The callback.</param>
        internal EngineThread(ThreadManager manager, string name, bool startImmediately, bool fixedTimeStep, Action<Timing> callback)
        {
            _manager = manager;
            _timing = new Timing(callback);
            _timing.IsFixedTimestep = fixedTimeStep;

            _thread = new Thread(() =>
            {
                while (!_shouldExit)
                    _timing.Update();
            })
            {
                Name = name,
            };
            _thread.Start();

            if (startImmediately)
                _timing.Start();

            Name = name;
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
    }
}
