using Molten.Collections;

namespace Molten.Graphics
{
    internal partial class RenderChain : IRenderChain
    {
        Link _first;
        RendererDX11 _renderer;
        internal readonly ObjectPool<Link> LinkPool;
        internal readonly ObjectPool<RenderChainContext> ContextPool;

        internal RenderChain(RendererDX11 renderer)
        {
            _renderer = renderer;
            LinkPool = new ObjectPool<Link>(() => new Link(this));
            ContextPool = new ObjectPool<RenderChainContext>(() => new RenderChainContext(_renderer));
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
            RenderChainContext context = ContextPool.GetInstance();
            context.Layer = layerData as LayerRenderData<Renderable>;
            context.Scene = sceneData as SceneRenderData<Renderable>;
            context.Context = _renderer.Device.Context; // TODO for now, the immediate context will process all chain steps.

            _renderer.Surfaces.MultiSampleLevel = camera.MultiSampleLevel;

            if (camera.MultiSampleLevel >= AntiAliasLevel.X2)
                context.BaseStateConditions = StateConditions.Multisampling;

            _first.Run(_renderer, camera, context, time);
            Link.Recycle(_first);
        }
    }
}
