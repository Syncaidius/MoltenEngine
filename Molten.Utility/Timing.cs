using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten
{
    public class Timing
    {
        static TimeSpan SLEEP_THRESHOLD = TimeSpan.FromMilliseconds(5.0);

        int _targetUps;
        float _deltaTime;
        double _targetFrameTime;
        TimeSpan _accumulated;
        TimeSpan _upsTime;
        int _upsCurrent;
        int _ups;
        ulong _frame;

        Stopwatch _timer;
        TimeSpan _elapsed;
        TimeSpan _total;
        TimeSpan _target;
        Action<Timing> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timing"/> class.
        /// </summary>
        /// <param name="callback">The callback to be run on each update invocation.</param>
        public Timing(Action<Timing> callback, int targetUPS = 60)
        {
            _timer = new Stopwatch();
            _callback = callback;
            TargetUPS = targetUPS;
        }

        /// <summary>Starts the <see cref="Timing"/> instance.</summary>
        public void Start()
        {
            _timer.Start();
        }

        /// <summary>Stops the <see cref="Timing"/> instance.</summary>
        public void Pause()
        {
            _timer.Stop();
        }

        /// <summary>Resets the <see cref="Timing"/> instance.</summary>
        public void Reset()
        {
            _timer.Reset();
            _ups = 0;
            _upsCurrent = 0;
            _total = new TimeSpan();
            _elapsed = new TimeSpan();
            _upsTime = new TimeSpan();
            _accumulated = new TimeSpan();
        }

        /// <summary>Runs a timing update. Returns the number of updates needed to advance. If it falls behind, a value of more than 1 will be returned.</summary>
        /// <param name="fixedTimeStep">If true, Thread.Sleep will be called to sleep off excess/spare frame time.</param>
        /// <returns>The number of updates needed to catch up.</returns>
        public void Update()
        {
            // SEE: https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Game.cs

            if (_timer.IsRunning)
            {
                if (IsFixedTimestep)
                {
                    if (_timer.Elapsed >= _target)
                    {
                        // Accumulate time
                        _timer.Stop();
                        _accumulated += _timer.Elapsed;

                        // Set the elapsed and time-delta to what 1-frame would usually take.
                        _elapsed = _target;
                        _deltaTime = (float)(_elapsed.TotalMilliseconds / _target.TotalMilliseconds);
                        _timer.Restart();

                        // Do as many updates as we can within the accumulated time.
                        while (_accumulated >= _target)
                        {
                            _accumulated -= _target;
                            _total += _target;
                            DoUpdate();
                        }
                    }
                    else
                    {
                        // Check if it's worth sleeping to save CPU/battery.
                        TimeSpan remaining = _target - _accumulated;
                        if (remaining >= SLEEP_THRESHOLD)
                            Thread.Sleep(remaining);
                    }
                }
                else // .. Not fixed time-step. Run as fast as possible.
                {
                    _timer.Stop();
                    _elapsed = _timer.Elapsed;
                    _total += _elapsed;
                    _deltaTime = (float)(_elapsed.TotalMilliseconds / _targetFrameTime);
                    _timer.Restart();

                    DoUpdate();
                }
            }
            else
            {
                // Sleep for the length of one frame. 
                // Inaccuracy does not matter here since the timer isn't running. It should catch up easily once its running again.
                Thread.Sleep(_target);
            }
        }

        private void DoUpdate()
        {
            // Calculate UPS
            _upsTime += _elapsed;
            _upsCurrent++;
            _frame++;

            if (_upsTime.TotalMilliseconds >= 1000)
            {
                _ups = _upsCurrent;
                _upsTime -= TimeSpan.FromMilliseconds(1000);
                _upsCurrent = 0;
            }

            _callback(this);
        }

        /// <summary>
        /// Gets the time taken to complete the previous frame.
        /// </summary>
        public TimeSpan ElapsedTime => _elapsed;

        /// <summary>Gets the total elapsed time since the thread or application started.</summary>
        public TimeSpan TotalTime => _total;

        /// <summary>
        /// Gets the delta time.
        /// </summary>
        public float Delta => _deltaTime;

        /// <summary>
        /// Gets the current frame.
        /// </summary>
        public ulong CurrentFrame => _frame;

        /// <summary>Gets or sets whether the timer will maintain a fixed time-step equal to <see cref="TargetFrameTime"/>.</summary>
        public bool IsFixedTimestep { get; set; } = true;

        /// <summary>Gets the target frame time.</summary>
        public double TargetFrameTime => _targetFrameTime;

        /// <summary>
        /// Gets or sets the target UPS/FPS.
        /// </summary>
        public int TargetUPS
        {
            get => _targetUps;
            set
            {
                _targetUps = value;
                _targetFrameTime = 1000.0 / _targetUps;
                _target = TimeSpan.FromTicks((long)(_targetFrameTime * TimeSpan.TicksPerMillisecond));
            }
        }

        /// <summary>Gets the UPS within the snapshot of the last second. This is the same as <see cref="FPS"/></summary>
        public int UPS
        {
            get { return _ups; }
        }

        /// <summary>Gets the FPS within the snapshot of the last second. This is the same as <see cref="UPS"/></summary>
        public int FPS
        {
            get { return _ups; }
        }

        /// <summary>Gets whether the timing object is running.</summary>
        public bool IsRunning => _timer.IsRunning;

        /// <summary>Gets the amount of time lag the update cycles have accumulated. Each frame that takes longer than <see cref="TargetFrameTime"/> will increase lag.</summary>
        public TimeSpan AccumulatedLag => _accumulated;
    }
}
