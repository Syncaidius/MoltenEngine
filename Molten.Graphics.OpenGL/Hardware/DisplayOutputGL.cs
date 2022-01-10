using System;

namespace Molten.Graphics
{
    public class DisplayOutputGL : IDisplayOutput
    {
        GraphicsAdapterGL _adapter;
        string _name;

        internal DisplayOutputGL(GraphicsAdapterGL adapter, string name, Rectangle bounds)
        {
            _adapter = adapter;
            _name = name;
            DesktopBounds = bounds;
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
        public Rectangle DesktopBounds { get; }
    }
}
