using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>Handles localization of strings by storing them with a key alongside a translation for each supported language.</summary>
    public class LocalizationTable : LocalizationModule
    {
        /// <summary>The code of the default language.</summary>
        public const string DEFAULT_LANGUAGE = "en_us";
        const string REGEX_RULE = @"^\{.*?\}$";

        string _currentLanguage;
        CultureInfo _currentCulture;

        Dictionary<uint, LocalizationModule> _modules;
        
        /// <summary>Invoked when the localization culture has been changed via the <see cref="CurrentCulture"/> property.</summary>
        public event ObjectHandler<CultureInfo> OnCultureChanged;

        /// <summary>Creates a new instance of <see cref="LocalizationTable"/>.</summary>
        internal LocalizationTable()
        {
            //find out what the current culture/language is.
            _currentCulture = CultureInfo.CurrentUICulture;
            _currentLanguage = _currentCulture.Name;
            _modules = new Dictionary<uint, LocalizationModule>();
        }

        /// <summary>Adds a localization module to the table.</summary>
        /// <param name="moduleID">The module ID.</param>
        /// <param name="module">The localization module.</param>
        public void AddModule(uint moduleID, LocalizationModule module)
        {
            _modules.Add(moduleID, module);
        }

        /// <summary>Removes all unused strings from the localization table. Keeps strings for only the current and default language(s).</summary>
        public void PruneTable()
        {
            Prune(_currentLanguage);
            foreach (LocalizationModule m in _modules.Values)
                m.Prune(_currentLanguage);
        }

        /// <summary>Returns a string ifs format matches the </summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        public string Localize(string input)
        {
            if (Regex.IsMatch(input, REGEX_RULE))
            {
                // Cut the start and end characters off, which should be { and }.
                string key = input.Substring(1, input.Length - 2).ToLower();
                return GetString(_currentLanguage ,key);
            }

            return input;
        }

        /// <summary>Attempts to localize the input string using strings from a loaded <see cref="LocalizationModule"/></summary>
        /// <param name="moduleID"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Localize(uint moduleID, string input)
        {
            // Cut the start and end characters off, which should be { and }. This gives us a key.
            string key = input.Substring(1, input.Length - 2).ToLower();

            LocalizationModule mod = null;
            if (_modules.TryGetValue(moduleID, out mod))
            {
                if (Regex.IsMatch(input, REGEX_RULE))
                {
                    // Attempt to retrieve string from module. If it doesnt exist, retrieve from master table instead.
                    // This allows base localizations to be overridden by mods or extensions.
                    // This also keeps mod localizations isolated from each other.
                    LocalizedString ls = null;
                    if (mod.StringTable.TryGetValue(key, out ls))
                        return ls[_currentLanguage];
                    else
                        return GetString(_currentLanguage, key);
                }
            }

            return GetString(_currentLanguage, input);
        }

        /// <summary>Clears all strings from the localization table.</summary>
        public override void Clear()
        {
            base.Clear();
            _modules.Clear();
        }

        /// <summary>Gets the or sets the current culture. This will automatically change the current anguage.</summary>
        public CultureInfo CurrentCulture
        {
            get { return _currentCulture; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("CurrentCulture");

                _currentCulture = value;
                _currentLanguage = _currentCulture.Name;

                //trigger a culture changed event - any UI components subscribed to it will get the opportunity to update any localized text they display.
                OnCultureChanged?.Invoke(_currentCulture);
            }
        }

        /// <summary>Gets or sets the current language (e.g. en-US or en-GB). This will automatically set the current culture.
        /// See for language codes: http://msdn.microsoft.com/en-us/library/hh202918%28v=vs.92%29.aspx: </summary>
        public string CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                _currentLanguage = value;
                _currentCulture = new CultureInfo(_currentLanguage);

                // Trigger a culture changed event - any UI components subscribed to it will get the opportunity to update any localized text they display.
                OnCultureChanged?.Invoke(_currentCulture);
            }
        }
    }
}
