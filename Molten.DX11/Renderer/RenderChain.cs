using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RenderChain
    {
        RendererDX11 _renderer;
        DeferredRenderStep _first;
        DeferredRenderStep _last;
        SceneRenderDataDX11 _scene;

        internal RenderChain(RendererDX11 renderer, SceneRenderDataDX11 scene)
        {
            _renderer = renderer;
            _scene = scene;
        }

        private RenderChain Next(DeferredRenderStep step)
        {
            if(_first == null)
            {
                _first = step;
                _last = step;
            }

            _last.Next = step;
            _last = _last.Next;
            _last.Next = null;
            return this;
        }

        private RenderChain Next<T>() where T : DeferredRenderStep, new()
        {
            DeferredRenderStep step = _renderer.GetRenderStep<T>();
            return Next(step);
        }

        internal void Rebuild()
        {
            // TODO if the current scene has the same flags as the previous scene, skip rebuilding chain.
            // TODO consider moving/caching render chain construction in to SceneRenderDataDX11. If the flags are changed, (re)build it on the next render cycle.
            _first = null;
            _last = null;

            if (_scene.HasFlag(SceneRenderFlags.Deferred))
            {
                Next<GBuffer3dStep>();
                // TODO complete deferred chain here
            }
            else
            {
                if (_scene.HasFlag(SceneRenderFlags.Render3D))
                    Next<Immediate3dStep>();

                if (_scene.HasFlag(SceneRenderFlags.Render2D))
                    Next<Render2dStep>();
            }
        }

        internal void Render(SceneRenderDataDX11 scene, Timing time)
        {
            _first.Render(_renderer, scene, time);
        }
    }
}
