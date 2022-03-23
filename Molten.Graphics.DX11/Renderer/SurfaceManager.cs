using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal class SurfaceManager : IDisposable
    {
        ThreadedDictionary<string, SurfaceConfig> _surfacesByKey;
        ThreadedList<SurfaceConfig> _surfaces;
        ThreadedDictionary<MainSurfaceType, SurfaceConfig> _mainSurfaces;

        DepthStencilSurface _depthSurface;
        RendererDX11 _renderer;

        internal SurfaceManager(RendererDX11 renderer)
        {
            _surfacesByKey = new ThreadedDictionary<string, SurfaceConfig>();
            _mainSurfaces = new ThreadedDictionary<MainSurfaceType, SurfaceConfig>();
            _surfaces = new ThreadedList<SurfaceConfig>();
            _renderer = renderer;
        }

        public void Dispose()
        {
            _surfaces.For(0, 1, (index, config) => config.Surface.Dispose());
            _surfaces.Clear();
            _depthSurface.Dispose();
            _mainSurfaces.Clear();
            _surfacesByKey.Clear();
        }

        internal void Initialize(uint width, uint height)
        {
            InitializeMainSurfaces(width, height);
        }

        internal void CreateMainSurface(
            string key,
            MainSurfaceType mainType,
            uint width,
            uint height,
            GraphicsFormat format,
            SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            Format dxgiFormat = (Format)format;
            RenderSurface2D surface = new RenderSurface2D(_renderer, width, height, dxgiFormat, name: $"renderer_{key}");
            SurfaceConfig config = RegisterSurface(key, surface, sizeMode);
            _mainSurfaces[mainType] = config;
        }

        internal void InitializeMainSurfaces(uint width, uint height)
        {
            CreateMainSurface("scene", MainSurfaceType.Scene, width, height, GraphicsFormat.R8G8B8A8_UNorm);
            CreateMainSurface("normals", MainSurfaceType.Normals, width, height, GraphicsFormat.R11G11B10_Float);
            CreateMainSurface("emissive", MainSurfaceType.Emissive, width, height, GraphicsFormat.R8G8B8A8_UNorm);
            CreateMainSurface("composition1", MainSurfaceType.Composition1, width, height, GraphicsFormat.R16G16B16A16_Float);
            CreateMainSurface("composition2", MainSurfaceType.Composition2, width, height, GraphicsFormat.R16G16B16A16_Float);
            CreateMainSurface("lighting", MainSurfaceType.Lighting, width, height, GraphicsFormat.R16G16B16A16_Float);
            _depthSurface = new DepthStencilSurface(_renderer, width, height, DepthFormat.R24G8_Typeless);
        }

        internal void Rebuild(uint requiredWidth, uint requiredHeight)
        {
            _surfaces.For(0, 1, (index, config) => config.RefreshSize(requiredWidth, requiredHeight));
            _depthSurface.Resize(requiredWidth, requiredHeight);
        }

        internal SurfaceConfig RegisterSurface(string key, RenderSurface2D surface, SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            key = key.ToLower();
            if (!_surfacesByKey.TryGetValue(key, out SurfaceConfig config))
            {
                config = new SurfaceConfig(surface, sizeMode);
                _surfacesByKey.Add(key, config);
                _surfaces.Add(config);
            }

            return config;
        }

        internal T Get<T>(MainSurfaceType type) where T : RenderSurface2D
        {
            return _mainSurfaces[type].Surface as T;
        }

        internal T Get<T>(string key) where T : RenderSurface2D
        {
            return _surfacesByKey[key].Surface as T;
        }

        internal DepthStencilSurface GetDepth()
        {
            return _depthSurface;
        }
    }
}
