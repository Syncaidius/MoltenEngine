using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten
{
    public abstract class SettingBank : EngineObject
    {
        ThreadedList<SettingValue> _settings;
        ThreadedDictionary<string, SettingValue> _byKey;

        public SettingBank()
        {
            _settings = new ThreadedList<SettingValue>();
            _byKey = new ThreadedDictionary<string, SettingValue>();
        }

        public void Log(Logger log, string title)
        {
            log.WriteLine($"{title} settings:");
            foreach(KeyValuePair<string, SettingValue>  p in _byKey)
                log.WriteLine($"\t {p.Key}: {p.Value}");
        }

        protected bool RemoveSetting(string key)
        {
            SettingValue r = null;
            return _byKey.TryRemoveValue(key, out r);
        }

        protected SettingValue<T> AddSetting<T>(string key, T defaultValue = default(T))
        {
            SettingValue<T> r = new SettingValue<T>();
            r.SetSilently(defaultValue);

            _settings.Add(r);
            _byKey.TryAdd(key, r);
            return r;
        }

        protected SettingValueList<T> AddSettingList<T>(string key)
        {
            SettingValueList<T> r = new SettingValueList<T>();

            _settings.Add(r);
            _byKey.TryAdd(key, r);
            return r;
        }

        /// <summary>Apply all pending setting changes.</summary>
        public void Apply()
        {
            _settings.ForInterlock(0, 1, (index, item) =>
            {
                item.Apply();
                return false;
            });
        }

        /// <summary>Cancel all pending setting changes.</summary>
        public void Cancel()
        {
            _settings.ForInterlock(0, 1, (index, item) =>
            {
                item.Cancel();
                return false;
            });
        }
    }
}
