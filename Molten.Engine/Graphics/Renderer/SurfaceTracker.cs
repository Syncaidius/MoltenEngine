
namespace Molten.Graphics
{
    public class SurfaceTracker : IDisposable
    {
        SurfaceSizeMode _mode;
        Dictionary<AntiAliasLevel, IRenderSurface2D> _surfaces;

        GraphicsDevice _device;
        uint _width;
        uint _height;
        GraphicsFormat _format;
        string _name;

        internal SurfaceTracker(GraphicsDevice device,
            AntiAliasLevel[] aaLevels,
            uint width,
            uint height,
            GraphicsFormat format,
            string name,
            SurfaceSizeMode mode = SurfaceSizeMode.Full)
        {
            _device = device;
            _width = width;
            _height = height;
            _format = format;
            _name = name;
            _mode = mode;
            _surfaces = new Dictionary<AntiAliasLevel, IRenderSurface2D>();
        }

        internal void RefreshSize(uint minWidth, uint minHeight)
        {
            _width = minWidth;
            _height = minHeight;

            switch (_mode)
            {
                case SurfaceSizeMode.Full:
                    foreach(IRenderSurface2D rs in _surfaces.Values)
                        rs?.Resize(minWidth, minHeight);
                    break;

                case SurfaceSizeMode.Half:
                    foreach (IRenderSurface2D rs in _surfaces.Values)
                        rs?.Resize((minWidth / 2) + 1, (minHeight / 2) + 1);
                    break;
            }
        }

        private IRenderSurface2D Create(AntiAliasLevel aa)
        {
            IRenderSurface2D rs = _device.CreateSurface(_width, _height, _format, 1, 1, aa, TextureFlags.None, $"{_name}_{aa}aa");
            _surfaces[aa] = rs;
            return rs;
        }

        public void Dispose()
        {
            foreach(IRenderSurface2D rs in _surfaces.Values)
                rs.Dispose();
        }

        internal IRenderSurface2D this[AntiAliasLevel aaLevel]
        {
            get
            {
                if (!_surfaces.TryGetValue(aaLevel, out IRenderSurface2D rs))
                {
                    rs = Create(aaLevel);
                    _surfaces[aaLevel] = rs;
                }
                else if (rs.Width != _width || rs.Height != _height)
                {
                    rs.Resize(_width, _height);
                }

                return rs;
            }
        }
    }
}
