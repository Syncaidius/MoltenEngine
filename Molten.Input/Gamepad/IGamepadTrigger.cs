using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{ 
    /// <summary>
    /// Represents a trigger or pressure button on a gamepad.
    /// </summary>
    public interface IGamepadTrigger
    {
        float Value { get; }

        float Delta { get; }
    }
}
