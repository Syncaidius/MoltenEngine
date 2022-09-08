using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Threading;
using Newtonsoft.Json.Linq;

namespace Molten.Audio
{
    public delegate void AudioDeviceChangedHandler<T>(T oldDevice, T newDevice) where T : IAudioDevice;

    public abstract class AudioService : EngineService
    {
        public event AudioDeviceChangedHandler<IAudioInput> InputChanged;

        public event AudioDeviceChangedHandler<IAudioOutput> OutputChanged;

        bool _shouldUpdate;
        ThreadedList<AudioBuffer> _buffers;

        IAudioInput _curInput;
        IAudioOutput _curOutput;

        protected AudioService()
        {
            _buffers = new ThreadedList<AudioBuffer>();
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            UpdateDeviceSwitching();
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            _shouldUpdate = true;
            return ThreadingMode.SeparateThread;
        }

        protected override void OnStop()
        {
            _shouldUpdate = false;
        }

        protected override sealed void OnUpdate(Timing time)
        {
            if (!_shouldUpdate)
                return;

            UpdateDeviceSwitching();
            OnUpdateAudioEngine(time);
        }

        private void UpdateDeviceSwitching()
        {
            if (Input != _curInput)
            {
                SwitchInput(_curInput, Input);
                InputChanged?.Invoke(_curInput, Input);
                _curInput = Input;
            }

            if (Output != _curOutput)
            {
                SwitchOutput(_curOutput, Output);
                OutputChanged?.Invoke(_curOutput, Output);
                _curOutput = Output;
            }
        }

        /// <summary>
        /// Creates a new <see cref="IAudioBuffer"/>
        /// </summary>
        /// <returns></returns>
        public AudioBuffer CreateBuffer(uint bufferSize, AudioFormat format, uint frequency)
        {
            AudioBuffer buffer = new AudioBuffer(bufferSize, format, frequency);
            buffer.OnDisposed += Buffer_OnDisposed;
            _buffers.Add(buffer);
            return buffer;
        }

        private void Buffer_OnDisposed(AudioBuffer obj)
        {
            _buffers.Remove(obj as AudioBuffer);
        }

        protected override void OnServiceDisposing()
        {
            base.OnServiceDisposing();

            Log.Warning($"Disposing {_buffers.Count} leftover audio buffers. These should be properly disposed!");
            for (int i = _buffers.Count - 1; i >= 0; i--)
                _buffers[i].Dispose();
        }

        protected abstract void OnUpdateAudioEngine(Timing time);

        protected abstract void SwitchInput(IAudioInput oldDevice, IAudioInput newDevice);

        protected abstract void SwitchOutput(IAudioOutput oldDevice, IAudioOutput newDevice);

        /// <summary>
        /// Gets a list of all detected <see cref="IAudioDevice"/>.
        /// </summary>
        public abstract IReadOnlyList<IAudioDevice> AvailableDevices { get; }

        /// <summary>
        /// Gets a list of all available <see cref="IAudioInput"/> devices.
        /// </summary>
        public abstract IReadOnlyList<IAudioInput> AvailableInputDevices { get; }

        /// <summary>
        /// Gets a list of all available <see cref="IAudioOutput"/> devices.
        /// </summary>
        public abstract IReadOnlyList<IAudioOutput> AvailableOutputDevices { get; }

        /// <summary>
        /// Gets or sets <see cref="IAudioInput"/> used by the current application.
        /// </summary>
        public IAudioInput Input { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IAudioOutput"/> used by the current application.
        /// </summary>
        public IAudioOutput Output { get; set; }
    }
}
