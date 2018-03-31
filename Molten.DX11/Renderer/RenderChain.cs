using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RenderChain
    {
        internal class Link
        {
            public Link Previous;
            public Link Next;
            public RenderStepBase Step;
        }

        RendererDX11 _renderer;
        Link _first;
        Link _last;
        SceneRenderDataDX11 _scene;

        internal RenderChain(RendererDX11 renderer, SceneRenderDataDX11 scene)
        {
            _renderer = renderer;
            _scene = scene;
        }

        private void Next(RenderStepBase step)
        {
            Link link = new Link() { Step = step };
            if (_first == null)
            {
                _first = link;
                _last = _first;
            }
            else
            {
                link.Previous = _last;
                _last.Next = link;
                _last = link;
                _last.Next = null;
            }
        }

        private void Next<T>() where T : RenderStepBase, new()
        {
            RenderStepBase step = _renderer.GetRenderStep<T>();
            Next(step);
        }

        internal void Rebuild()
        {
            // TODO if the current scene has the same flags as the previous scene, skip rebuilding chain.
            // TODO consider moving/caching render chain construction in to SceneRenderDataDX11. If the flags are changed, (re)build it on the next render cycle.
            _first = null;
            _last = null;

            Next<StartStep>();

            if (_scene.HasFlag(SceneRenderFlags.Deferred))
            {
                Next<GBuffer3dStep>();
                //Next<FinalizeStep>();
                Next<Render2dStep>();
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
            Link link = _first;
            while(link != null)
            {
                link.Step.Render(_renderer, scene, time, link);
                link = link.Next;
            }
        }
    }
}
