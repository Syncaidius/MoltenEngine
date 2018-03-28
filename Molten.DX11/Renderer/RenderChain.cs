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

        internal RenderChain(RendererDX11 renderer)
        {
            _renderer = renderer;
        }

        internal void Clear()
        {
            _first = null;
            _last = null;
        }

        internal RenderChain Next(DeferredRenderStep step)
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

        internal RenderChain Next<T>() where T : DeferredRenderStep, new()
        {
            DeferredRenderStep step = _renderer.GetRenderStep<T>();
            return Next(step);
        }

        internal void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time)
        {
            scene.PreRender(renderer, renderer.Device);
            _first.Render(renderer, scene, time);
            scene.PostRender(renderer);
        }
    }
}
