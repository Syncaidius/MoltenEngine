using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class InputSettings : SettingBank
    {
        public const string DEFAULT_LIBRARY = "Molten.Input.Windows.dll; Molten.Input.InputManager";

        public InputSettings()
        {
            InputLibrary = AddSetting<string>("input", DEFAULT_LIBRARY);
        }

        /// <summary>Gets the input library to use with the engine. This can be changed to any library containing one or more implementations of <see cref="IInputManager"/>.</summary>
        [DataMember]
        public SettingValue<string> InputLibrary { get; private set; }
    }
}
