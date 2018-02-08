using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Molten
{
    public class LocalizedString
    {
        Dictionary<string, string> _stringTable;
        string _description; //no use except to translators + UI editor

        public LocalizedString()
        {
            _stringTable = new Dictionary<string,string>();
        }

        /// <summary>Removes all strings except the ones needed for the default and current language.</summary>
        /// <param name="currentLanguage">The current language being used by the app/game.</param>
        /// <param name="parentTable">The table to use when performing the prune.</param>
        internal void Prune(string currentLanguage)
        {
            //store the count for when the dictionary is recreated with a fixed-size
            int stringCount = _stringTable.Count;

            //drop all strings except the default and current
            string cur = string.Empty;
            string def = string.Empty;

            if (HasLanguage(currentLanguage))
                cur = _stringTable[currentLanguage];

            string defaultLang = LocalizationTable.DEFAULT_LANGUAGE;

            if (HasLanguage(defaultLang))
                def = _stringTable[defaultLang];

            _stringTable.Clear();

            //create with enough space to hold the current and default strings only (2)
            _stringTable = new Dictionary<string, string>(2);

            //readd default and current
            SetLocalizedString(currentLanguage, cur);
            SetLocalizedString(defaultLang, def);
        }

        public void SetLocalizedString(string language, string localizedStr)
        {
            //if a string already exists for the given language, update it. If not, add it.
            if (_stringTable.ContainsKey(language) == false)
            {
                if(string.IsNullOrWhiteSpace(localizedStr) == false)
                    _stringTable.Add(language, localizedStr);
            }
            else
                _stringTable[language] = localizedStr;
        }

        public bool HasLanguage(string language)
        {
            return _stringTable.ContainsKey(language);
        }

        /// <summary>Returns a string for the given language. If it isn't found, the string for LocalizedString.DefaultLanguage is returned.</summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public string this[string language]
        {
            get
            {
                string output = string.Empty;

                //try to get the string for the provided language.
                _stringTable.TryGetValue(language.ToLower(), out output);

                //if no string was found for the provided language, return the string for the default language.
                if (string.IsNullOrWhiteSpace(output) == false)
                    return output;
                else
                    return _stringTable[LocalizationTable.DEFAULT_LANGUAGE];
            }

            set
            {
                if (_stringTable.ContainsKey(language))
                    _stringTable[language] = value;
                else
                    if (string.IsNullOrWhiteSpace(value) == false)
                        _stringTable.Add(language, value);
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public object Tag { get; set; }
    }
}
