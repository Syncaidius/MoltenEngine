using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace Molten.Audio.OpenAL
{
    public class SoundInstance : OpenALObject, ISoundInstance
    {
        uint _alSourceID;
        AudioPlaybackState _state;
        AudioPlaybackState _requestedState;
        bool _looping;
        bool _created;

        internal unsafe SoundInstance(SoundSource source) : base(source.Service)
        {
            ParentSource = source;

            uint src = 0;
            Service.Al.GenSources(1, &src);
            if (CheckAlError($"Failed to create AL source for instance {Name}"))
                return;

            // Check if looping was enabled by the native drivers/implementation.
            Service.Al.GetSourceProperty(_alSourceID, SourceBoolean.Looping, out _looping);
            if (CheckAlError($"Failed to retrieve looping status '{Source.Name}' instance {_alSourceID}"))
                return;

            _alSourceID = src;
            _created = true;
            SetSource(source);
        }

        public void SetSource(ISoundSource source)
        {
            if (source is SoundSource src)
            {
                AudioPlaybackState curState = _state;
                Stop(); // Stop playing before changing the buffer

                Service.Al.SetSourceProperty(_alSourceID, SourceInteger.Buffer, src.AlBufferID);
                ParentSource = src;

                if (curState == AudioPlaybackState.Playing)
                    Play();
                else if (curState == AudioPlaybackState.Paused)
                    Pause();
            }
        }

        public void QueueSource(ISoundSource source)
        {
            // See Page 9: http://open-activewrl.sourceforge.net/data/OpenAL_PGuide.pdf
            throw new NotImplementedException();
        }

        public void QueueSources(ISoundSource[] sources)
        {
            // See Page 9: http://open-activewrl.sourceforge.net/data/OpenAL_PGuide.pdf
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            if(_created)
            {
                Service.Al.DeleteSource(_alSourceID);
                _alSourceID = 0;
                _created = false;
            }

            _state = AudioPlaybackState.Disposed;
        }

        public void Play()
        {            
            _requestedState = AudioPlaybackState.Playing;
        }

        public void Pause()
        {
            _requestedState = AudioPlaybackState.Paused;
        }

        public void Stop()
        {
            _requestedState = AudioPlaybackState.Stopped;
        }

        internal unsafe void Update()
        {
            int playState = 0;
            Service.Al.GetSourceProperty(_alSourceID, GetSourceInteger.SourceState, &playState);
            if (!CheckAlError($"Failed to retrieve sound instance state for '{Name}' -- Source: '{Source.Name}'"))
            {
                if (_looping != IsLooping)
                {
                    _looping = IsLooping;
                    Service.Al.SetSourceProperty(_alSourceID, SourceBoolean.Looping, _looping);
                    AudioError result = Service.Al.GetError();
                    CheckAlError($"Failed to set looping on '{Source.Name}' instance {_alSourceID}");
                }

                // Ensure we have the latest state from the source, in case the native implementation decides to automatically change it.
                switch ((SourceState)playState)
                {
                    case SourceState.Initial: break;
                    case SourceState.Playing: _state = AudioPlaybackState.Playing; break;
                    case SourceState.Paused: _state = AudioPlaybackState.Paused; break;
                    case SourceState.Stopped: _state = AudioPlaybackState.Stopped; break;
                }

                if (_requestedState != _state)
                {
                    _state = _requestedState;
                    switch (_state)
                    {
                        case AudioPlaybackState.Playing: Service.Al.SourcePlay(_alSourceID); break;
                        case AudioPlaybackState.Paused: Service.Al.SourcePause(_alSourceID); break;
                        case AudioPlaybackState.Stopped: Service.Al.SourceStop(_alSourceID); break;
                    }
                }
            }
            else
            {
                // TODO delete the sound instance from it's source
            }
        }



        public ISoundSource Source => ParentSource;

        public SoundSource ParentSource { get; private set; }

        public AudioPlaybackState State => _state; 

        public bool IsLooping { get; set; }

        public int QueuedSourceCount => throw new NotImplementedException();
    }
}
