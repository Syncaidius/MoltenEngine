using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class DisplayOutputGL : IDisplayOutput
    {
        DisplayDevice _device;
        GraphicsAdapterGL _adapter;
        string _name;

        internal DisplayOutputGL(GraphicsAdapterGL adapter, DisplayDevice device, DisplayIndex index)
        {
            _device = device;
            _adapter = adapter;
            _name = index.ToString();
        }

        /// <summary>
        /// Gets the name of the display output.
        /// </summary>
        public string Name => _name;

        /// <summary>Gets the orientation of the current <see cref="IDisplayOutput"/>.</summary>
        public DisplayOrientation Orientation => throw new NotImplementedException();

        /// <summary>Gets the <see cref="IDisplayAdapter"/> the <see cref="IDisplayOutput"/> is attached to.</summary>
        public IDisplayAdapter Adapter => _adapter;

        /// <summary>
        /// Gets the desktop bounds of the display output.
        /// </summary>
        public Rectangle DesktopBounds => _device.Bounds.FromApi();
    }
}
