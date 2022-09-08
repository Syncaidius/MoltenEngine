using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        internal ThreadedList<SoundInstance> Instances { get; }
        bool _created;

        internal unsafe SoundSource(OutputDevice device) : base(device.Service)
        {
            Instances = new ThreadedList<SoundInstance>();
            ParentDevice = device;
            Output = device;

            uint bID = 0;
            Service.Al.GenBuffers(1, &bID);
            if (!CheckAlError($"Failed to generate AL buffer for sound source '{Name}'"))
            {
                AlBufferID = bID;
                _created = true;
            }
        }

        public ISoundInstance CreateInstance()
        {
            SoundInstance instance = new SoundInstance(this);
            Instances.Add(instance);
            return instance;
        }

        protected override void OnDispose()
        {
            if (_created)
            {
                Service.Al.DeleteBuffer(AlBufferID);
                AlBufferID = 0;
                _created = false;
            }
        }

        /// <summary>
        /// Commits a <see cref="IAudioBuffer"/> and all of it's data.
        /// </summary>
        /// <param name="buffer"></param>
        public unsafe void CommitBuffer(AudioBuffer buffer)
        {
            if (!_created)
                throw new Exception($"The underlying OpenAL source buffer was not created yet for '{Name}'");

            Service.Al.BufferData(AlBufferID, buffer.Format.ToApi(), buffer.PtrStart, (int)buffer.Size, (int)buffer.Frequency);
            CheckAlError($"Failed to commit buffer '{buffer.Name}' to sound source '{Name}'");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        /// <param name="numSamples"></param>
        /// <exception cref="Exception"></exception>
        public unsafe void CommitBuffer(AudioBuffer buffer, uint position, int numSamples)
        {
            if (!_created)
                throw new Exception($"The underlying OpenAL source buffer was not created yet for '{Name}'");

            byte* ptrData = buffer.PtrStart + position;

            if (position >= buffer.Size)
                throw new Exception("The position exceeds the size of the provided audio buffer");

            Service.Al.BufferData(AlBufferID, buffer.Format.ToApi(), ptrData, numSamples, (int)buffer.Frequency);
            CheckAlError($"Failed to commit buffer '{buffer.Name}' to sound source '{Name}'");
        }

        internal OutputDevice ParentDevice { get; }

        internal uint AlBufferID { get; private set; }

        public int InstanceCount => Instances.Count;

        public IAudioOutput Output { get; }
    }
}
