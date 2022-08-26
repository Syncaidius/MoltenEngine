using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Threading;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace Molten.Audio.OpenAL
{
    public class AudioServiceAL : AudioService
    {
        internal const string ALC_ENUMERATION_EXT = "ALC_enumeration_EXT";

        AL _al;
        ALContext _context;

        public AudioServiceAL()
        {

        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            Log.WriteLine($"Initializing OpenAL");
            _al = AL.GetApi(true);
            _context = ALContext.GetApi(true);
            
            
            List<DeviceInfo> info = GetAvailableDevices();
        }

        private unsafe List<DeviceInfo> GetAvailableDevices()
        {
            List<DeviceInfo> available = new List<DeviceInfo>();

            if (_context.IsExtensionPresent(ALC_ENUMERATION_EXT))
            {
                bool success = _context.TryGetExtension<Enumeration>(null, out Enumeration enumeration);
                if (success)
                {
                    string defaultName = enumeration.GetString(null, GetEnumerationContextString.DefaultDeviceSpecifier);
                    Log.WriteLine($"Default audio output: {defaultName}");

                    Log.WriteLine("Available audio outputs:");
                    IEnumerable<string> list = enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers);
                    int id = 0;

                    foreach (string deviceName in list)
                    {
                        Log.WriteLine($"\t{id++}: {deviceName}");
                        // TODO populate device info list.
                        // if deviceName == defaultName, set DeviceInfo.IsDefault to true
                    }
                }
                else
                {
                    Log.Error($"Unable to detect devices due to missing extension: {ALC_ENUMERATION_EXT}");
                }
            }

            return available;
        }

        protected override void OnUpdateAudioEngine(Timing time)
        {

        }

        protected override void OnDispose()
        {
            Log.Write("Shutting down OpenAL");
            base.OnDispose();

            _context.Dispose();
            _al.Dispose();
        }
    }
}
