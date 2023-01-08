using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Molten.Graphics
{
    public unsafe class DisplayOutputVK : IDisplayOutput
    {
        Monitor* _monitor;
        Rectangle _bounds;

        internal DisplayOutputVK(DisplayManagerVK manager, Monitor* monitor)
        {
            Manager = manager;
            _monitor = monitor;
            _bounds = Rectangle.Empty;

            Name = manager.Renderer.GLFW.GetMonitorName(_monitor);
            manager.Renderer.GLFW.GetMonitorWorkarea(_monitor, out _bounds.Left, out _bounds.Top, out int bWidth, out int bHeight);
            _bounds.Width = bWidth;
            _bounds.Height = bHeight;
        }

        public IDisplayMode[] GetSupportedModes(GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }

        public DisplayOrientation Orientation => throw new NotImplementedException();

        public IDisplayAdapter Adapter => Adapter;

        internal Monitor* Ptr => _monitor;

        public Rectangle DesktopBounds => _bounds;

        internal DisplayAdapterVK AssociatedAdapter { get; set; }

        internal DisplayManagerVK Manager { get; }
    }
}
