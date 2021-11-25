using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class EngineService : EngineObject
    {
        public abstract void Initialize(SettingBank settings, Logger log);

        public abstract void Start();

        public bool IsInitialized { get; protected set; }
    }

    public abstract class EngineService<T> : EngineService
        where T : SettingBank
    {
        public event MoltenEventHandler<EngineService<T>, T> OnInitialized;

        public override sealed void Initialize(SettingBank settings, Logger log)
        {
            Settings = settings as T;
            try
            {
                log.WriteLine($"Initializing service: {this.GetType()}");
                OnInitialize(Settings, log);
                log.WriteLine($"Completed initialization of service: {this.GetType()}");
            }
            catch(Exception ex)
            {
                log.WriteLine($"Failed to initialize service: {this.GetType()}");
                log.WriteError(ex);
            }
            OnInitialized.Invoke(this, Settings);
            IsInitialized = true;
        }

        protected abstract void OnInitialize(T settings, Logger log);

        public T Settings { get; private set; }
    }
}
