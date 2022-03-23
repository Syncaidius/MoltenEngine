using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal class SurfaceTracker : IDisposable
    {
        SurfaceSizeMode _mode;
        RenderSurface2D[] _surfaces;
        RendererDX11 _renderer;

        uint _width;
        uint _height;
        Format _dxgiFormat;
        string _name;

        internal SurfaceTracker(
            RendererDX11 renderer,
            AntiAliasLevel[] aaLevels,
            uint width,
            uint height,
            Format dxgiFormat,
            string name,
            SurfaceSizeMode mode = SurfaceSizeMode.Full)
        {
            _width = width;
            _height = height;
            _dxgiFormat = dxgiFormat;
            _name = name;
            _mode = mode;
            _renderer = renderer;
            _surfaces = new RenderSurface2D[aaLevels.Length];
        }

        internal void RefreshSize(uint minWidth, uint minHeight)
        {
            _width = minWidth;
            _height = minHeight;

            switch (_mode)
            {
                case SurfaceSizeMode.Full:
                    foreach(RenderSurface2D rs in _surfaces)
                        rs?.Resize(minWidth, minHeight);
                    break;

                case SurfaceSizeMode.Half:
                    foreach (RenderSurface2D rs in _surfaces)
                        rs?.Resize((minWidth / 2) + 1, (minHeight / 2) + 1);
                    break;
            }
        }

        internal void Create(AntiAliasLevel aa)
        {
            _surfaces[(uint)aa] = new RenderSurface2D(
                _renderer, 
                _width, 
                _height, 
                _dxgiFormat, 
                name: $"{_name}_aa{aa}", 
                aaLevel: aa
            );
        }

        public void Dispose()
        {
            for (int i = 0; i < _surfaces.Length; i++)
                _surfaces[i].Dispose();
        }


        internal RenderSurface2D this[AntiAliasLevel aaLevel]
        {
            get
            {
                uint aaID = (uint)aaLevel;
                if (_surfaces[aaID] == null)
                    Create(aaLevel);
                else if (_surfaces[aaID].Width != _width || _surfaces[aaID].Height != _height)
                    _surfaces[aaID].Resize(_width, _height);

                return _surfaces[aaID];
            }
        }
    }

    public enum SurfaceSizeMode : byte
    {
        /// <summary>
        /// The surface will be at least the width and height of the largest-rendered surface.
        /// </summary>
        Full = 0,

        /// <summary>
        /// The surface will be at least half the width and height of the largest-rendered surface.
        /// </summary>
        Half = 1,

        /// <summary>
        /// The surface will remain at a fixed size regardless of resolution changes.
        /// </summary>
        Fixed = 2,
    }
}
