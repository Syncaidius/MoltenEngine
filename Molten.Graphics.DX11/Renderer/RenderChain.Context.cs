using Molten.Collections;

namespace Molten.Graphics
{
    internal partial class RenderChain
    {
        internal class Context : IPoolable
        {
            internal SceneRenderData<Renderable> Scene;
            internal LayerRenderData<Renderable> Layer;

            RenderSurface2D[] _composition;
            int _curComposition;

            internal Context(RendererDX11 renderer)
            {
                _composition = new RenderSurface2D[]
                {
                    renderer.Surfaces[MainSurfaceType.Composition1],
                    renderer.Surfaces[MainSurfaceType.Composition2]
                };
            }

            internal void SwapComposition()
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
            internal RenderSurface2D CompositionSurface => _composition[_curComposition];

            /// <summary>
            /// Gets the previous composition surface.
            /// </summary>
            internal RenderSurface2D PreviousComposition => _composition[1 - _curComposition];

            /// <summary>
            /// Gets whether or not a composition surface was used at some point.
            /// </summary>
            internal bool HasComposed { get; private set; }

            internal StateConditions BaseStateConditions { get; set; }
        }
    }
}
