using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public class InputSettings : SettingBank
    {
        internal static string DEFAULT_LIBRARY = "Molten.IO.Windows.dll; Molten.IO.InputManager";

        public InputSettings()
        {
            InputLibrary = AddSetting<string>("input", DEFAULT_LIBRARY);
        }

        /// <summary>Gets the input library to use with the engine. This can be changed to any library containing one or more implementations of <see cref="IInputManager"/>.</summary>
        [DataMember]
        public SettingValue<string> InputLibrary { get; private set; }
    }
}
