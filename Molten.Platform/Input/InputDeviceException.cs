using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Platform.Input
{
    public class InputDeviceException : Exception
    {
        public InputDeviceException(string message, IInputDevice device) : base(message)
        {
            Device = device;
        }

        public IInputDevice Device { get; }
    }
}
