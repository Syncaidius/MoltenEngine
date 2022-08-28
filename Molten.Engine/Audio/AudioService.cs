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

        IAudioInput _input;
        IAudioOutput _output;

        IAudioInput _newInput;
        IAudioOutput _newOutput;

        protected override void OnInitialize(EngineSettings settings)
        {
            
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

            if(_newInput != _input)
            {
                SwitchInput(_input, _newInput);
                InputChanged?.Invoke(_input, _newInput);
                _input = _newInput;
            }

            if (_newOutput != _output)
            {
                SwitchOutput(_output, _newOutput);
                OutputChanged?.Invoke(_output, _newOutput);
                _output = _newOutput;
            }

            OnUpdateAudioEngine(time);
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
        public IAudioInput Input
        {
            get => _newInput;
            set => _newInput = value;
        }

        /// <summary>
        /// Gets or sets <see cref="IAudioOutput"/> used by the current application.
        /// </summary>
        public IAudioOutput Output
        {
            get => _newOutput;
            set => _newOutput = value;
        }
    }
}
