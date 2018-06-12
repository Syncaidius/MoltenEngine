using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{ 
    /// <summary>
    /// Represents a thumb-stick or joystick of a game controller.
    /// </summary>
    public interface IGamepadStick
    {
        Vector2F Value { get; }

        float X { get; }

        float Y { get; }

        Vector2F Delta { get; }

        float DeltaX { get; }

        float DeltaY { get; }
    }
}
