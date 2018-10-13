using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal partial class RenderChain : IRenderChain
    {
        Link _first;
        readonly RendererDX11 _renderer;
        internal readonly ObjectPool<Link> LinkPool;
        internal readonly ObjectPool<Context> ContextPool;

        internal RenderChain(RendererDX11 renderer)
        {
            _renderer = renderer;
            LinkPool = new ObjectPool<Link>(() => new Link(this));
            ContextPool = new ObjectPool<Context>(() => new Context(_renderer));
        }

        public void Build(SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
        {
            _first = LinkPool.GetInstance();
            _first.Set<StartStep>();

            if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
                _first.Next<GBufferStep>().Next<LightingStep>().Next<CompositionStep>().Next<SkyboxStep>().Next<FinalizeStep>();
            else
                _first.Next<ForwardStep>().Next<SkyboxStep>().Next<FinalizeStep>();
        }

        public void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time)
        {
            Context context = ContextPool.GetInstance();
            context.Layer = layerData as LayerRenderData<Renderable>;
            context.Scene = sceneData as SceneRenderData<Renderable>;

            _first.Run(_renderer, camera, context, time);
            Link.Recycle(_first);
        }
    }
}
