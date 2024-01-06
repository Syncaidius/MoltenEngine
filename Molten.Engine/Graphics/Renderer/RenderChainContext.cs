using Molten.Collections;

namespace Molten.Graphics;

internal class RenderChainContext : IPoolable
{
    internal SceneRenderData Scene;
    internal LayerRenderData Layer;

    IRenderSurface2D[] _composition;
    int _curComposition;

    public RenderChainContext(RenderService renderer)
    {
        _composition = [
            renderer.Surfaces[MainSurfaceType.Composition1],
            renderer.Surfaces[MainSurfaceType.Composition2]
        ];
    }

    public void SwapComposition()
    {
        HasComposed = true;
        _curComposition = 1 - _curComposition;
    }

    public void ClearForPool()
    {
        _curComposition = 0;
        HasComposed = false;
        Scene = null;
        Layer = null;
    }

    /// <summary>
    /// Gets the composition surface that should be used next.
    /// </summary>
    public IRenderSurface2D CompositionSurface => _composition[_curComposition];

    /// <summary>
    /// Gets the previous composition surface.
    /// </summary>
    public IRenderSurface2D PreviousComposition => _composition[1 - _curComposition];

    /// <summary>
    /// Gets whether or not a composition surface was used at some point.
    /// </summary>
    public bool HasComposed { get; private set; }
}
