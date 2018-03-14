using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    /// <summary>
    /// Represents an implementation of a mouse or pointer device.
    /// </summary>
    public interface IMouseDevice : IInputDevice<MouseButton>
    {
        /// <summary>Positions the mouse cursor at the center of the window.</summary>
        void CenterInWindow();

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        Vector2F Moved { get; }

        /// <summary>Gets the amount the mouse wheel has been moved since the last frame.</summary>
        float WheelDelta { get; }

        /// <summary>Gets the current scroll wheel position.</summary>
        float WheelPosition { get; }

        /// <summary>
        /// Gets the position of the mouse cursor.
        /// </summary>
        Vector2F Position { get; }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        bool CursorVisible { get; set; }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        bool IsConstrained { get; set; }
    }
}
