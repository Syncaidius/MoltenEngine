using Molten.Collections;
using System.Collections;
using System.Collections.Concurrent;

namespace Molten
{
    public abstract class SettingBank
    {
        ThreadedList<SettingValue> _settings;
        ConcurrentDictionary<string, SettingValue> _byKey;

        internal SettingBank()
        {
            _settings = new ThreadedList<SettingValue>();
            _byKey = new ConcurrentDictionary<string, SettingValue>();
        }

        public void Log(Logger log, string title)
        {
            log.WriteLine($"{title} settings:");
            foreach (KeyValuePair<string, SettingValue> p in _byKey)
            {
                string msg = "";
                if (!(p.Value.Object is string) && p.Value.Object is IEnumerable enumerable)
                {
                    msg = $"\t {p.Key}: ";
                    bool first = true;
                    foreach (object obj in enumerable)
                    {
                        if (!first)
                            msg += ", ";
                        else
                            first = false;

                        msg += $"{obj.ToString()}";
                    }
                }
                else
                {
                    msg = $"\t {p.Key}: {p.Value.Object}";
                }

                log.WriteLine(msg);
            }
        }

        protected bool RemoveSetting(string key)
        {
            SettingValue r = null;
            return _byKey.Remove(key, out r);
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
            _settings.For(0, 1, (index, item) => item.Apply());
        }

        /// <summary>Cancel all pending setting changes.</summary>
        public void Cancel()
        {
            _settings.For(0, 1, (index, item) => item.Cancel());
        }
    }
}
