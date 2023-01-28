using System.Collections;
using System.Collections.Concurrent;

namespace Molten
{
    public abstract class SettingBank
    {
        List<SettingValue> _settings;
        List<SettingBank> _childBanks;
        ConcurrentDictionary<string, SettingValue> _byKey;

        internal SettingBank()
        {
            _settings = new List<SettingValue>();
            _childBanks = new List<SettingBank>();
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

        protected T AddBank<T>()
            where T : SettingBank, new()
        {
            T bank = new T();
            _childBanks.Add(bank);
            return bank;
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

        /// <summary>Apply all pending setting changes, including those in child/nested <see cref="SettingBank"/>.</summary>
        public void Apply()
        {
            foreach (SettingValue val in _settings)
                val.Apply();

            foreach(SettingBank bank in _childBanks)
                bank.Apply();
        }

        /// <summary>Cancel all pending setting changes, including those in child/nested <see cref="SettingBank"/>.</summary>
        public void Cancel()
        {
            foreach (SettingValue val in _settings)
                val.Cancel();

            foreach (SettingBank bank in _childBanks)
                bank.Cancel();
        }
    }
}
