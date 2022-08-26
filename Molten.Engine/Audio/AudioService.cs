using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Threading;

namespace Molten.Audio
{
    public abstract class AudioService : EngineService
    {
        bool _shouldUpdate;
        ThreadedList<ISoundSource> _sources;

        protected override void OnInitialize(EngineSettings settings)
        {
            _sources = new ThreadedList<ISoundSource>();
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            _shouldUpdate = true;
            return ThreadingMode.SeparateThread;
        }

        protected override void OnStop()
        {
            _shouldUpdate = false;
        }

        protected override sealed void OnUpdate(Timing time)
        {
            if (!_shouldUpdate)
                return;

            OnUpdateAudioEngine(time);
        }

        protected abstract void OnUpdateAudioEngine(Timing time);
    }
}
