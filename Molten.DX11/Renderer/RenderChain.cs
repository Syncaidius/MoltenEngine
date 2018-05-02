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
            public RenderChain Chain;
        }

        internal Link First;
        internal Link Last;

        RendererDX11 _renderer;
        SceneRenderDataDX11 _scene;

        internal RenderChain(RendererDX11 renderer, SceneRenderDataDX11 scene)
        {
            _renderer = renderer;
            _scene = scene;
        }

        private void Next(RenderStepBase step)
        {
            Link link = new Link() { Chain = this, Step = step };
            if (First == null)
            {
                First = link;
                Last = First;
            }
            else
            {
                link.Previous = Last;
                Last.Next = link;
                Last = link;
                Last.Next = null;
            }
        }

        private void Next<T>() where T : RenderStepBase, new()
        {
            RenderStepBase step = _renderer.GetRenderStep<T>();
            Next(step);
        }

        internal void Rebuild()
        {
            First = null;
            Last = null;

            Next<StartStep>();

            if (_scene.HasFlag(SceneRenderFlags.Deferred))
            {
                if (_scene.HasFlag(SceneRenderFlags.Render3D))
                    Next<GBuffer3dStep>();

                if (_scene.HasFlag(SceneRenderFlags.Render2D))
                    Next<Render2dStep>();

                Next<LightingStep>();
                Next<FinalizeStep>();
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
            Link link = First;
            while(link != null)
            {
                link.Step.Render(_renderer, scene, time, link);
                link = link.Next;
            }
        }
    }
}
