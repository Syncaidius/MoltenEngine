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
        public const string DEFAULT_LIBRARY = "Molten.Platform.Windows.dll; Molten.Input.InputManager";

        public InputSettings()
        {
            Library = AddSetting<string>("input", DEFAULT_LIBRARY);
        }

        /// <summary>Gets the input library to use with the engine. This can be changed to any library containing one or more implementations of <see cref="IInputManager"/>.</summary>
        [DataMember]
        public SettingValue<string> Library { get; private set; }
    }
}
