using Molten.Collections;

namespace Molten.Graphics
{
    internal partial class RenderChainDX11 : IRenderChain
    {
        RenderChainLink _first;
        internal readonly ObjectPool<RenderChainLink> LinkPool;
        internal readonly ObjectPool<RenderChainContext> ContextPool;

        internal RenderChainDX11(RendererDX11 renderer)
        {
            Renderer = renderer;
            LinkPool = new ObjectPool<RenderChainLink>(() => new RenderChainLink(this));
            ContextPool = new ObjectPool<RenderChainContext>(() => new RenderChainContext(Renderer));
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

            Renderer.Surfaces.MultiSampleLevel = camera.MultiSampleLevel;

            if (camera.MultiSampleLevel >= AntiAliasLevel.X2)
                context.BaseStateConditions = StateConditions.Multisampling;

            _first.Run(Renderer, camera, context, time);
            RenderChainLink.Recycle(_first);
        }

        internal RendererDX11 Renderer { get; }
    }
}
