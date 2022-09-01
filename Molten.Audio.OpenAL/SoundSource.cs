using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace Molten.Audio.OpenAL
{
    public class SoundSource : OpenALObject, ISoundSource
    {
        ThreadedList<SoundInstance> _instances;
        uint _alBufferID;
        bool _created;

        internal unsafe SoundSource(OutputDevice device) : base(device.Service)
        {
            _instances = new ThreadedList<SoundInstance>();

            ParentDevice = device;
            Output = device;

            uint bID = 0;
            Service.Al.GenBuffers(1, &bID);
            AudioError result = Service.Al.GetError();
            if (!CheckAlError($"Failed to generate AL buffer for sound source '{Name}'"))
            {
                _alBufferID = bID;
                _created = true;
            }
        }

        public ISoundInstance GetInstance()
        {
            SoundInstance instance = new SoundInstance(this);
            _instances.Add(instance);
            return instance;
        }

        protected override void OnDispose()
        {
            if (_created)
            {
                Service.Al.DeleteBuffer(_alBufferID);
                _alBufferID = 0;
                _created = false;
            }
        }

        public void CommitBuffer(IAudioBuffer buffer)
        {
            throw new NotImplementedException();
        }

        internal OutputDevice ParentDevice { get; }

        internal uint AlBufferID { get; private set; }

        public int InstanceCount => _instances.Count;

        public IAudioOutput Output { get; }
    }
}
