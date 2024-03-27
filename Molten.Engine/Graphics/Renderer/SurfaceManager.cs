using Molten.Collections;
using System.Collections.Concurrent;

namespace Molten.Graphics;

public class SurfaceManager : IDisposable
{
    ConcurrentDictionary<string, SurfaceTracker> _surfacesByKey;
    ThreadedList<SurfaceTracker> _surfaces;
    SurfaceTracker[] _mainSurfaces;
    HashSet<IRenderSurface2D> _firstCleared;

    DepthSurfaceTracker _depthSurface;
    RenderService _renderer;
    AntiAliasLevel[] _aaLevels;

    internal SurfaceManager(RenderService renderer)
    {
        _aaLevels = ReflectionHelper.GetEnumValues<AntiAliasLevel>();
        MainSurfaceType[] surfaceTypes = ReflectionHelper.GetEnumValues<MainSurfaceType>();

        _firstCleared = new HashSet<IRenderSurface2D>();
        _surfacesByKey = new ConcurrentDictionary<string, SurfaceTracker>();
        _mainSurfaces = new SurfaceTracker[surfaceTypes.Length];
        _surfaces = new ThreadedList<SurfaceTracker>();
        _renderer = renderer;
    }

    public void Dispose()
    {
        _surfaces.For(0, (index, config) => config.Dispose());
        _surfaces.Clear();
        _depthSurface.Dispose();
        _surfacesByKey.Clear();
    }

    internal void Initialize(uint width, uint height)
    {
        RegisterMainSurface(MainSurfaceType.Scene, width, height, GpuResourceFormat.R8G8B8A8_UNorm);
        RegisterMainSurface(MainSurfaceType.Normals, width, height, GpuResourceFormat.R11G11B10_Float);
        RegisterMainSurface(MainSurfaceType.Emissive, width, height, GpuResourceFormat.R8G8B8A8_UNorm);
        RegisterMainSurface(MainSurfaceType.Composition1, width, height, GpuResourceFormat.R16G16B16A16_Float);
        RegisterMainSurface(MainSurfaceType.Composition2, width, height, GpuResourceFormat.R16G16B16A16_Float);
        RegisterMainSurface(MainSurfaceType.Lighting, width, height, GpuResourceFormat.R16G16B16A16_Float);
        _depthSurface = new DepthSurfaceTracker(_renderer.Device, _aaLevels, width, height, DepthFormat.R24G8);
    }

    internal void ClearIfFirstUse(GpuCommandList cmd, IRenderSurface2D surface, Color color)
    {
        if (surface == null)
            return;

        if(!_firstCleared.Contains(surface))
        {
            surface.ClearImmediate(cmd, color);
            _firstCleared.Add(surface);
        }
    }

    internal void ResetFirstCleared()
    {
        _firstCleared.Clear();
    }

    internal void RegisterMainSurface(
        MainSurfaceType mainType,
        uint width,
        uint height,
        GpuResourceFormat format,
        SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
    {
        string key = mainType.ToString();
        SurfaceTracker tracker = RegisterSurface(key, width, height, format, sizeMode);
        _mainSurfaces[(uint)mainType] = tracker;
    }

    internal void Rebuild(uint requiredWidth, uint requiredHeight)
    {
        _surfaces.For(0, (index, config) => config.RefreshSize(requiredWidth, requiredHeight));
        _depthSurface.RefreshSize(requiredWidth, requiredHeight);
    }

    internal SurfaceTracker RegisterSurface(string key, uint width, uint height, GpuResourceFormat format, SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
    {
        key = key.ToLower();
        if (!_surfacesByKey.TryGetValue(key, out SurfaceTracker config))
        {
            config = new SurfaceTracker(_renderer.Device, _aaLevels, width, height, format, key, sizeMode);
            _surfacesByKey.TryAdd(key, config);
            _surfaces.Add(config);
        }

        return config;
    }

    public IRenderSurface2D this[MainSurfaceType type] => _mainSurfaces[(uint)type][MultiSampleLevel];

    public IRenderSurface2D this[string key] => _surfacesByKey[key][MultiSampleLevel];

    public IDepthStencilSurface GetDepth() => _depthSurface[MultiSampleLevel];

    public AntiAliasLevel MultiSampleLevel { get; set; } = AntiAliasLevel.None;
}
