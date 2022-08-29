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
    public class SoundInstance : ISoundInstance
    {
        public event ObjectHandler<ISoundInstance> OnDisposing;

        public event ObjectHandler<ISoundInstance> OnDisposed;

        uint _alSourceID;
        AudioPlaybackState _state;
        bool _looping;

        internal unsafe SoundInstance(SoundSource source)
        {
            ParentSource = source;
            Service = ParentSource.ParentDevice.Service;

            uint src = 0;
            Service.Al.GenSources(1, &src);

            AudioServiceAL service = Service;
            AudioError result = service.Al.GetError();
            if (result != AudioError.NoError)
            {
                string msg = service.GetErrorMessage(result);
                service.Log.Error($"Failed to create sound instance from source '{source.Name}': {msg}");
            }
            else
            {
                _alSourceID = src;
                Service.Al.SetSourceProperty(_alSourceID,SourceInteger.Buffer, source.AlBufferID);
            }
        }

        public void SetSource(ISoundSource source)
        {
            throw new NotImplementedException();
        }

        public void QueueSource(ISoundSource source)
        {
            throw new NotImplementedException();
        }

        public void QueueSources(ISoundSource[] sources)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            OnDisposing?.Invoke(this);
            OnDisposed?.Invoke(this);
            _state = AudioPlaybackState.Disposed;
        }

        public void Play(bool loop = false)
        {
            if (_state == AudioPlaybackState.Playing)
                return;

            IsLooping = true;
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
