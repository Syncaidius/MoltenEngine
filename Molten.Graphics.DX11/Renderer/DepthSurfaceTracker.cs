using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal class DepthSurfaceTracker : IDisposable
    {
        SurfaceSizeMode _mode;
        Dictionary<AntiAliasLevel, DepthStencilSurface> _surfaces;
        RendererDX11 _renderer;

        uint _width;
        uint _height;
        DepthFormat _format;

        internal DepthSurfaceTracker(
            RendererDX11 renderer,
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
            _renderer = renderer;
            _surfaces = new Dictionary<AntiAliasLevel, DepthStencilSurface>();
        }

        internal void RefreshSize(uint minWidth, uint minHeight)
        {
            _width = minWidth;
            _height = minHeight;

            switch (_mode)
            {
                case SurfaceSizeMode.Full:
                    foreach(DepthStencilSurface rs in _surfaces.Values)
                        rs?.Resize(minWidth, minHeight);
                    break;

                case SurfaceSizeMode.Half:
                    foreach (DepthStencilSurface rs in _surfaces.Values)
                        rs?.Resize((minWidth / 2) + 1, (minHeight / 2) + 1);
                    break;
            }
        }

        internal DepthStencilSurface Create(AntiAliasLevel aa)
        {
            DepthStencilSurface rs = new DepthStencilSurface(
                _renderer, 
                _width, 
                _height, 
                _format, 
                name: $"depth_{aa}aa", 
                aaLevel: aa
            );

            _surfaces[aa] = rs;
            return rs;
        }

        public void Dispose()
        {
            foreach(DepthStencilSurface rs in _surfaces.Values)
                rs.Dispose();
        }

        internal DepthStencilSurface this[AntiAliasLevel aaLevel]
        {
            get
            {
                if (!_surfaces.TryGetValue(aaLevel, out DepthStencilSurface rs))
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
