namespace Molten.Graphics
{
    internal class DepthSurfaceTracker : IDisposable
    {
        SurfaceSizeMode _mode;
        Dictionary<AntiAliasLevel, IDepthStencilSurface> _surfaces;
        GraphicsDevice _device;

        uint _width;
        uint _height;
        DepthFormat _format;

        internal DepthSurfaceTracker(
            GraphicsDevice device,
            AntiAliasLevel[] aaLevels,
            uint width,
            uint height,
            DepthFormat format,
            SurfaceSizeMode mode = SurfaceSizeMode.Full)
        {
            _width = width;
            _height = height;
            _format = format;
            _mode = mode;
            _device = device;
            _surfaces = new Dictionary<AntiAliasLevel, IDepthStencilSurface>();
        }

        internal void RefreshSize(uint minWidth, uint minHeight)
        {
            _width = minWidth;
            _height = minHeight;

            switch (_mode)
            {
                case SurfaceSizeMode.Full:
                    foreach(IDepthStencilSurface rs in _surfaces.Values)
                        rs?.Resize(minWidth, minHeight);
                    break;

                case SurfaceSizeMode.Half:
                    foreach (IDepthStencilSurface rs in _surfaces.Values)
                        rs?.Resize((minWidth / 2) + 1, (minHeight / 2) + 1);
                    break;
            }
        }

        internal IDepthStencilSurface Create(AntiAliasLevel aa)
        {
            IDepthStencilSurface ds = _device.CreateDepthSurface(_width, _height, _format, 1, 1, aa, TextureFlags.None, $"depth_{aa}aa");
            _surfaces[aa] = ds;
            return ds;
        }

        public void Dispose()
        {
            foreach(IDepthStencilSurface rs in _surfaces.Values)
                rs.Dispose();
        }

        internal IDepthStencilSurface this[AntiAliasLevel aaLevel]
        {
            get
            {
                if (!_surfaces.TryGetValue(aaLevel, out IDepthStencilSurface rs))
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
