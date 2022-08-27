using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Threading;
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
        ALContext _context;
        List<OutputDevice> _outputs;
        List<InputDevice> _inputs;
        List<AudioDevice> _devices;


        public AudioServiceAL()
        {
            _inputs = new List<InputDevice>();
            _outputs = new List<OutputDevice>();
            _devices = new List<AudioDevice>();

            Inputs = _inputs.AsReadOnly();
            Outputs = _outputs.AsReadOnly();
            Devices = _devices.AsReadOnly();
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            Log.WriteLine($"Initializing OpenAL");
            _al = AL.GetApi(true);
            _context = ALContext.GetApi(true);

            DetectDevices();
        }

        private unsafe void DetectDevices()
        {
            if (_context.TryGetExtension(null, out Enumeration enumeration))
            {
                // Get output devices
                string defaultName = enumeration.GetString(null, GetEnumerationContextString.DefaultDeviceSpecifier);
                Log.WriteLine($"Default audio output: {defaultName}");

                Log.WriteLine($"Available audio outputs:");
                IEnumerable<string> list = enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers);
                int id = 0;

                foreach (string deviceName in list)
                {
                    Log.WriteLine($"\t{id++}: {deviceName}");
                    bool isDefault = deviceName == defaultName;
                    _outputs.Add(new OutputDevice(this, deviceName, isDefault));
                }

                // Get input devices
                defaultName = enumeration.GetString(null, (GetEnumerationContextString)GetCaptureEnumString.DefaultCaptureDeviceSpecifier);
                Log.WriteLine($"Default audio input: {defaultName}");

                Log.WriteLine($"Available audio inputs:");
                list = enumeration.GetStringList((GetEnumerationContextStringList)GetCaptureEnumStringList.CaptureDeviceSpecifiers);
                id = 0;
                foreach (string deviceName in list)
                {
                    Log.WriteLine($"\t{id++}: {deviceName}");
                    bool isDefault = deviceName == defaultName;
                    _inputs.Add(new InputDevice(this, deviceName, isDefault));
                }

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

        protected override void OnDispose()
        {
            base.OnDispose();

            _context.Dispose();
            _al.Dispose();
        }

        public override IReadOnlyList<IAudioDevice> Devices { get; }

        public override IReadOnlyList<IAudioInput> Inputs { get; }

        public override IReadOnlyList<IAudioOutput> Outputs { get; }
    }
}
