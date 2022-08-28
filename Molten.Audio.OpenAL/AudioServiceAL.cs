using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Threading;
using Newtonsoft.Json.Linq;
using Silk.NET.Core.Attributes;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Enumeration;

using GetCaptureEnumString = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureEnumerationContextString;
using GetCaptureEnumStringList = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureContextStringList;

namespace Molten.Audio.OpenAL
{
    public class AudioServiceAL : AudioService
    {
        AL _al;
        ALContext _alc;
        List<OutputDevice> _outputs;
        List<InputDevice> _inputs;
        List<AudioDevice> _devices;

        InputDevice _defaultInputDevice;
        OutputDevice _defaultOutputDevice;

        InputDevice _inputDevice;
        OutputDevice _outputDevice;

        public AudioServiceAL()
        {
            _inputs = new List<InputDevice>();
            _outputs = new List<OutputDevice>();
            _devices = new List<AudioDevice>();

            AvailableInputDevices = _inputs.AsReadOnly();
            AvailableOutputDevices = _outputs.AsReadOnly();
            AvailableDevices = _devices.AsReadOnly();
        }

        protected unsafe override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            _al = AL.GetApi(true);
            _alc = ALContext.GetApi(true);

            // Get OpenAL-Soft version
            int vMajor;
            int vMinor;
            _alc.GetContextProperty(null, GetContextInteger.MajorVersion, 1, &vMajor);
            _alc.GetContextProperty(null, GetContextInteger.MinorVersion, 1, &vMinor);
            Version = new Version(vMajor, vMinor);
            Log.WriteLine($"Initialized OpenAL-Soft {Version}");

            DetectDevices(settings);

            SwitchInput(null, _inputDevice);
            SwitchOutput(null, _outputDevice);
        }         

        private unsafe void DetectDevices(EngineSettings settings)
        {
            Log.WriteLine($"Input device from settings: {settings.Audio.InputDevice.Value}");
            Log.WriteLine($"Output device from settings: {settings.Audio.OutputDevice.Value}");

            if (_alc.TryGetExtension(null, out Enumeration enumeration))
            {
                // Get input devices
                string defaultName = enumeration.GetString(null, (GetEnumerationContextString)GetCaptureEnumString.DefaultCaptureDeviceSpecifier);
                Log.WriteLine($"Default audio input: {defaultName}");

                Log.WriteLine($"Available audio inputs:");
                IEnumerable<string> list = enumeration.GetStringList((GetEnumerationContextStringList)GetCaptureEnumStringList.CaptureDeviceSpecifiers);
                int id = 0;
                foreach (string deviceName in list)
                {
                    Log.WriteLine($"\t{id++}: {deviceName}");
                    bool isDefault = deviceName == defaultName;

                    InputDevice iDevice = new InputDevice(this, deviceName, isDefault);
                    _inputs.Add(iDevice);

                    if (isDefault)
                        _defaultInputDevice = iDevice;

                    if (iDevice.Name == settings.Audio.InputDevice.Value)
                        _inputDevice = iDevice;
                }

                // Get output devices
                defaultName = enumeration.GetString(null, GetEnumerationContextString.DefaultDeviceSpecifier);
                Log.WriteLine($"Default audio output: {defaultName}");

                Log.WriteLine($"Available audio outputs:");
                list = enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers);
                id = 0;

                foreach (string deviceName in list)
                {
                    Log.WriteLine($"\t{id++}: {deviceName}");
                    bool isDefault = deviceName == defaultName;
                    OutputDevice oDevice = new OutputDevice(this, deviceName, isDefault);
                    _outputs.Add(oDevice);

                    if (isDefault)
                        _defaultOutputDevice = oDevice;

                    if (oDevice.Name == settings.Audio.OutputDevice.Value)
                        _outputDevice = oDevice;
                }

                // Use default devices if we don't have 
                _inputDevice = _inputDevice ?? _defaultInputDevice;
                _outputDevice = _outputDevice ?? _defaultOutputDevice;

                // Update settings if the provided values were no longer valid.
                if(_inputDevice.Name != settings.Audio.InputDevice.Value)
                    settings.Audio.InputDevice.Value = _inputDevice.Name;

                if (_outputDevice.Name != settings.Audio.OutputDevice.Value)
                    settings.Audio.OutputDevice.Value = _outputDevice.Name;

                // Build a list of all avaialble devices.
                _devices.AddRange(_outputs);
                _devices.AddRange(_inputs);
            }
            else
            {
                string oal_ext = ExtensionAttribute.GetExtensionAttribute(typeof(Enumeration)).Name;
                Log.Error($"Unable to detect audio devices due to missing extension: {oal_ext}");
            }
        }      

        protected override void OnUpdateAudioEngine(Timing time)
        {

        }

        protected override void SwitchInput(IAudioInput oldDevice, IAudioInput newDevice)
        {
            InputDevice dOld = oldDevice as InputDevice;
            InputDevice dNew = newDevice as InputDevice;
            SwitchDevice(dOld, dNew);
            _inputDevice = dNew;
        }

        protected override void SwitchOutput(IAudioOutput oldDevice, IAudioOutput newDevice)
        {
            OutputDevice dOld = oldDevice as OutputDevice;
            OutputDevice dNew = newDevice as OutputDevice;
            SwitchDevice(dOld, dNew);
            _outputDevice = dNew;
        }

        private void SwitchDevice<T>(T oldDevice, T newDevice)
            where T : AudioDevice
        {
            if (oldDevice != null)
            {
                if (newDevice != null)
                {
                    newDevice.Open();
                    oldDevice.TransferTo(newDevice);
                }

                oldDevice.Close();
            }
            else
            {
                newDevice?.Open();
            }
        }

        protected override void OnServiceDisposing()
        {
            foreach (AudioDevice device in _devices)
                device.Dispose();

            _alc.Dispose();
            _al.Dispose();
        }

        public override IReadOnlyList<IAudioDevice> AvailableDevices { get; }

        public override IReadOnlyList<IAudioInput> AvailableInputDevices { get; }

        public override IReadOnlyList<IAudioOutput> AvailableOutputDevices { get; }

        internal InputDevice ActiveInput => _inputDevice;

        internal OutputDevice ActiveOutput => _outputDevice;

        internal AL Api => _al;

        internal ALContext Context => _alc;
    }
}
