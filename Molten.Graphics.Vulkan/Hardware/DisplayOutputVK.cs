using Silk.NET.GLFW;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Molten.Graphics
{
    public unsafe class DisplayOutputVK : IDisplayOutput
    {
        Monitor* _monitor;
        Rectangle _bounds;
        List<DisplayModeVK> _modes;

        internal DisplayOutputVK(DisplayManagerVK manager, Monitor* monitor)
        {
            Manager = manager;
            _monitor = monitor;
            _bounds = Rectangle.Empty;

            Name = manager.Renderer.GLFW.GetMonitorName(_monitor);
            manager.Renderer.GLFW.GetMonitorWorkarea(_monitor, out _bounds.Left, out _bounds.Top, out int bWidth, out int bHeight);
            _bounds.Width = bWidth;
            _bounds.Height = bHeight;

            CacheDisplayModes();
        }

        private void CacheDisplayModes()
        {
            _modes = new List<DisplayModeVK>();

            int modeCount = 0;
            VideoMode* modes = Manager.Renderer.GLFW.GetVideoModes(_monitor, out modeCount);
            for (int i = 0; i < modeCount; i++)
                _modes.Add(new DisplayModeVK(modes++));
        }

        public IReadOnlyList<IDisplayMode> GetModes(GraphicsFormat format)
        {
            return _modes.AsReadOnly();
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
