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
    public class SoundInstance : EngineObject, ISoundInstance
    {
        uint _alSourceID;
        AudioPlaybackState _state;
        bool _looping;

        internal unsafe SoundInstance(SoundSource source)
        {
            ParentSource = source;
            Service = ParentSource.ParentDevice.Service;

            uint src = 0;
            Service.Al.GenSources(1, &src);

            // Check if looping was enabled by the native drivers/implementation.
            Service.Al.GetSourceProperty(_alSourceID, SourceBoolean.Looping, out _looping);
            AudioError result = Service.Al.GetError();
            if (result != AudioError.NoError)
            {
                string msg = Service.GetErrorMessage(result);
                Service.Log.Error($"Failed to retrieve looping status '{Source.Name}' instance {_alSourceID}: {msg}");
            }

            // Set source if there are no errors.
            result = Service.Al.GetError();
            if (result != AudioError.NoError)
            {
                string msg = Service.GetErrorMessage(result);
                Service.Log.Error($"Failed to create sound instance from source '{source.Name}': {msg}");
            }
            else
            {
                _alSourceID = src;
                SetSource(source);
            }
        }

        public void SetSource(ISoundSource source)
        {
            if (source is SoundSource src)
            {
                AudioPlaybackState curState = _state;

                // Stop playing before changing the buffer
                if (curState != AudioPlaybackState.Stopped)
                    Stop();

                Service.Al.SetSourceProperty(_alSourceID, SourceInteger.Buffer, src.AlBufferID);

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
            if(_alSourceID > 0)
            {
                Service.Al.DeleteSource(_alSourceID);
                _alSourceID = 0;
            }

            _state = AudioPlaybackState.Disposed;
        }

        public void Play()
        {
            if (_state == AudioPlaybackState.Playing)
                return;

            Service.Al.SourcePlay(_alSourceID);
            _state = AudioPlaybackState.Playing;
        }

        public void Pause()
        {
            if (_state != AudioPlaybackState.Playing)
                return;

            Service.Al.SourcePause(_alSourceID);
            _state = AudioPlaybackState.Paused;
        }

        public void Stop()
        {
            if (_state == AudioPlaybackState.Stopped)
                return;

            Service.Al.SourceStop(_alSourceID);
            _state = AudioPlaybackState.Stopped;
        }

        internal unsafe void Update()
        {
            int playState = 0;
            Service.Al.GetSourceProperty(_alSourceID, GetSourceInteger.SourceState, &playState);

            switch ((SourceState)playState)
            {
                case SourceState.Initial: break;
                case SourceState.Playing: _state = AudioPlaybackState.Playing; break;
                case SourceState.Paused: _state = AudioPlaybackState.Paused; break;
                case SourceState.Stopped: _state = AudioPlaybackState.Stopped; break;
            }

            if(_looping != IsLooping)
            {
                _looping = IsLooping;
                Service.Al.SetSourceProperty(_alSourceID, SourceBoolean.Looping, _looping);
                AudioError result = Service.Al.GetError();
                if (result != AudioError.NoError)
                {
                    string msg = Service.GetErrorMessage(result);
                    Service.Log.Error($"Failed to set looping on '{Source.Name}' instance {_alSourceID}: {msg}");
                }
            }
        }

        internal AudioServiceAL Service { get; }

        public ISoundSource Source => ParentSource;

        public SoundSource ParentSource { get; }

        public AudioPlaybackState State => _state; 

        public bool IsLooping { get; set; }

        public int QueuedSourceCount => throw new NotImplementedException();
    }
}
