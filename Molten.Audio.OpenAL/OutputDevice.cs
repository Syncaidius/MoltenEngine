using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    internal unsafe class OutputDevice : AudioDevice, IAudioOutput
    {
        Context* _context;
        ThreadedList<ISoundSource> _sources;

        internal OutputDevice(AudioServiceAL service, string specifier, bool isDefault) :
            base(service, specifier, isDefault, AudioDeviceType.Output)
        {
            _sources = new ThreadedList<ISoundSource>();
        }

        protected override unsafe bool OnOpen()
        {
            Ptr = Service.Alc.OpenDevice(Name);
            ContextError result = Service.Alc.GetError(Ptr);
            if (CheckAlcError($""))
                return false;

            _context = Service.Alc.CreateContext(Ptr, null);
            result = Service.Alc.GetError(Ptr);
            if (result == ContextError.NoError)
            {
                Service.Alc.MakeContextCurrent(_context);
                Service.Alc.ProcessContext(_context);
                result = Service.Alc.GetError(Ptr);
            }

            return true;
        }

        protected override void OnClose()
        {
            Context* curContext = Service.Alc.GetCurrentContext();
            if (CheckAlcError($"Unable to retrieve the current context for '{Name}'"))
                return;

            if (curContext == _context)
            {
                Service.Alc.MakeContextCurrent(null);
                if (CheckAlcError($"Failed to unset '{Name}' as a the current context"))
                    return;
            }

            Service.Alc.SuspendContext(_context);
            Service.Alc.DestroyContext(_context);
            if (CheckAlcError($"Failed to destroy the context for '{Name}'"))
                return;

            Service.Alc.CloseDevice(Ptr); 
            if (CheckAlcError($"Failed to close AL device for '{Name}'"))
                return;
        }

        protected override void OnTransferTo(AudioDevice other)
        {
            // TODO recreate all _sources and their instances on the target device.
        }

        protected override void OnUpdate(Timing time)
        {
            for(int i = _sources.Count - 1; i >=0; i--)
            {
                SoundSource src = _sources[i] as SoundSource;

                for (int s = src.InstanceCount - 1; s >= 0; s--)
                    src.Instances[s].Update();
            }
        }

        public ISoundSource CreateSoundSource(AudioBuffer dataBuffer = null)
        {
            SoundSource source = new SoundSource(this);
            _sources.Add(source);
            if(dataBuffer != null)
                source.CommitBuffer(dataBuffer);

            return source;
        }
    }
}
