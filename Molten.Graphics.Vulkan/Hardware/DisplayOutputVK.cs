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

        internal DisplayOutputVK(DisplayManagerVK manager, Monitor* monitor)
        {
            Manager = manager;
            _monitor = monitor;

            Name = manager.Renderer.GLFW.GetMonitorName(_monitor);
        }        
        
        public string Name { get; }

        public DisplayOrientation Orientation => throw new NotImplementedException();

        public IDisplayAdapter Adapter => Adapter;

        internal Monitor* Ptr => _monitor;

        public Rectangle DesktopBounds => throw new NotImplementedException();

        internal DisplayAdapterVK AssociatedAdapter { get; set; }

        internal DisplayManagerVK Manager { get; }
    }
}
