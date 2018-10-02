using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RenderChain : IRenderChain
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

        internal RenderChain(RendererDX11 renderer)
        {
            _renderer = renderer;
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

        public void Build(SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
        {
            First = null;
            Last = null;

            /* TODO:
             *   - Implement chain branching. One chain step can lead to 1 or more new steps. 
             *      -- If two steps lead back into the same single step, that step will wait until both proceeding ones are done before being executed.
             *      -- Each extra simultaneous step will be given to a deferred context to execute.
             *   - RenderChains should be stored on LayerRenderData objects and updated when their flags change, to avoid rebuilding them every frame, for every scene layer.
             *  
             * NOTES:
             *   - Immediate rendering needs to be dumped.
             *   - StandardMesh and Mesh need to be merged into Mesh, with the functionality of StandardMesh
             *   - Deferred rendering should be the main focus
             *   - 2D should be included in the GBuffer stage; Layer flags can be used to disable lighting if non-lit/post-processed 2D elements are required
             *   
             * 
             * 
             */

            Next<StartStep>();

            if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
            {
                Next<GBufferStep>();
                Next<LightingStep>();
                Next<FinalizeStep>();
            }
            else
            {
                Next<Immediate3dStep>();
            }
        }

        public void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time)
        {
            Link link = First;
            LayerRenderData<Renderable> layer = layerData as LayerRenderData<Renderable>;

            while(link != null)
            {
                link.Step.Render(_renderer, camera, sceneData, layer, time, link);
                link = link.Next;
            }
        }
    }
}
