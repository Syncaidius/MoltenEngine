using Molten.Collections;

namespace Molten.Graphics
{
    internal class RenderChain
    {
        RenderChainLink _first;
        internal readonly ObjectPool<RenderChainLink> LinkPool;
        internal readonly ObjectPool<RenderChainContext> ContextPool;

        internal RenderChain(RenderService renderer)
        {
            Renderer = renderer;
            LinkPool = new ObjectPool<RenderChainLink>(() => new RenderChainLink(this));
            ContextPool = new ObjectPool<RenderChainContext>(() => new RenderChainContext(Renderer));
        }

        internal void Build(SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
        {
            _first = LinkPool.GetInstance();
            _first.Set<StartStep>();

            Renderer.BuildRenderChain(_first, scene, layerData, camera);
        }

        internal void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time)
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

        internal RenderService Renderer { get; }
    }
}
