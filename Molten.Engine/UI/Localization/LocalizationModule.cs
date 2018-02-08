using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>An object for storing localized strings. Used in conjunction with LocalizationTable.</summary>
    [DataContract]
    public class LocalizationModule
    {
        protected Dictionary<string, LocalizedString> _strings;

        /// <summary>Creates a new instance of <see cref="LocalizationModule"/></summary>
        public LocalizationModule()
        {
            _strings = new Dictionary<string, LocalizedString>();
        }

        /// <summary>Removes all unused strings from the localization table. Keeps strings for only the current and default language(s).</summary>
        public void Prune(string currentLanguage)
        {
            foreach (LocalizedString str in _strings.Values)
                str.Prune(currentLanguage);
        }

        /// <summary>Returns true if the localization table contains the provided key.</summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public bool HasKey(string keyName)
        {
            if (keyName != null && keyName != string.Empty)
                return _strings.ContainsKey(keyName);
            else
                return false;
        }

        /// <summary>Adds a <see cref="LocalizedString"/> instance to the table.</summary>
        /// <param name="keyName">The key name.</param>
        /// <param name="str">The <see cref="LocalizedString"/> instance.</param>
        public void AddString(string keyName, LocalizedString str)
        {
            if (keyName == null || keyName == string.Empty) return;

            string lowKey = keyName.ToLower();

            if (_strings.ContainsKey(lowKey) == false)
            {
                _strings.Add(lowKey, str);
            }
            else
            {
                _strings[lowKey] = str;
            }
        }

        /// <summary>Adds a localized string to the table.</summary>
        /// <param name="keyName">The key name.</param>
        /// <param name="language">The language code. Formatted as [ISO-3166]-[ISO-639]. Example: en-us, en-gb, de-de, dk-dk.</param>
        /// <param name="text"></param>
        public void AddString(string keyName, string language, string text)
        {
            if (keyName == null || keyName == string.Empty)
                return;

            string lowKey = keyName.ToLower();

            LocalizedString result = null;

            if (_strings.TryGetValue(lowKey, out result) == false)
            {
                result = new LocalizedString();
                _strings.Add(lowKey, result);
            }

            //add the localized string
            result[language] = text;
        }

        protected string GetString(string language, string key)
        {
            LocalizedString result = null;
            //try to get the localized string for the current language
            if (_strings.TryGetValue(key, out result))
                return result[language];
            else
                return key;
        }

        /// <summary>Removes a localized string from the table, based on the provided key.</summary>
        /// <param name="key">The key name.</param>
        public void RemoveString(string key)
        {
            string lowKey = key.ToLower();
            if (_strings.ContainsKey(lowKey) == true)
                _strings.Remove(lowKey);
        }

        /// <summary>Clears all strings from the localization table.</summary>
        public virtual void Clear()
        {
            _strings.Clear();
        }

        [DataMember]
        /// <summary>Gets the complete localization string table.</summary>
        public Dictionary<string, LocalizedString> StringTable
        {
            get { return _strings; }
        }
    }
}
