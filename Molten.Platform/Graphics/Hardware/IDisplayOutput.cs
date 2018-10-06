using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An interface for a display device, usually attached to a <see cref="IDisplayAdapter"/> device.</summary>
    public interface IDisplayOutput
    {
        /// <summary>Gets the name of the current <see cref="IDisplayOutput"/>.</summary>
        string Name { get; }

        /// <summary>Gets the orientation of the current <see cref="IDisplayOutput"/>.</summary>
        DisplayOrientation Orientation { get; }

        /// <summary>Gets the <see cref="IDisplayAdapter"/> the <see cref="IDisplayOutput"/> is attached to.</summary>
        IDisplayAdapter Adapter { get; }

        /// <summary>
        /// Gets the desktop bounds of the display.
        /// </summary>
        Rectangle DesktopBounds { get; }
    }
}
