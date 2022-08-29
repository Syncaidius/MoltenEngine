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
    public class SoundSource : EngineObject, ISoundSource
    {
        ThreadedList<SoundInstance> _instances;
        int _alBufferID;

        internal SoundSource(OutputDevice device)
        {
            _instances = new ThreadedList<SoundInstance>();

            ParentDevice = device;
            Output = device;
        }

        public ISoundInstance GetInstance()
        {
            SoundInstance instance = new SoundInstance(this);
            _instances.Add(instance);
            return instance;
        }

        protected override void OnDispose()
        {
            
        }

        public void CommitBuffer(IAudioBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public int InstanceCount => _instances.Count;

        public IAudioOutput Output { get; }

        internal OutputDevice ParentDevice { get; }

        internal uint AlBufferID { get; private set; }
    }
}
