using Molten.Collections;

namespace Molten.Graphics;

internal class RenderChain : EngineObject
{
    Dictionary<Type, RenderStep> _steps;
    List<RenderStep> _stepList;
    internal readonly ObjectPool<RenderChainLink> LinkPool;
    internal readonly ObjectPool<RenderChainContext> ContextPool;

    internal RenderChain(RenderService renderer)
    {
        Renderer = renderer;
        LinkPool = new ObjectPool<RenderChainLink>(() => new RenderChainLink(this));
        ContextPool = new ObjectPool<RenderChainContext>(() => new RenderChainContext(Renderer));
        _steps = new Dictionary<Type, RenderStep>();
        _stepList = new List<RenderStep>();
    }

    internal T GetRenderStep<T>() where T : RenderStep, new()
    {
        Type t = typeof(T);
        RenderStep step;
        if (!_steps.TryGetValue(t, out step))
        {
            step = new T();
            step.Initialize(Renderer);
            _steps.Add(t, step);
            _stepList.Add(step);
        }

        return step as T;
    }

    private RenderChainLink BuildPreRender(SceneRenderData scene, RenderCamera camera)
    {
        RenderChainLink first = LinkPool.GetInstance();
        first.Set<StartStep>();
        return first;
    }

    private RenderChainLink BuildRender(SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
    {
        RenderChainLink first = LinkPool.GetInstance();

        if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
            first.Set<GBufferStep>();
        else
            first.Set<ForwardStep>();

        return first;
    }

    private RenderChainLink BuildPostRender(SceneRenderData scene, RenderCamera camera)
    {
        RenderChainLink first = LinkPool.GetInstance();

        if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
            first.Set<LightingStep>().Next<CompositionStep>().Next<SkyboxStep>().Next<FinalizeStep>();
        else
            first.Set<SkyboxStep>().Next<FinalizeStep>();

        return first;
    }

    internal void Render(GraphicsQueue queue, SceneRenderData sceneData, RenderCamera camera, Timing time)
    {
        RenderChainContext context = ContextPool.GetInstance();
        Renderer.Surfaces.MultiSampleLevel = camera.MultiSampleLevel;
        context.Scene = sceneData;

        queue.Begin();
        queue.BeginEvent($"Pre-Render");
        RenderChainLink stepPreRender = BuildPreRender(sceneData, camera);
        stepPreRender.Run(queue, camera, context, time);
        RenderChainLink.Recycle(stepPreRender);
        queue.EndEvent();

        for (int i = 0; i < sceneData.Layers.Count; i++)
        {
            SceneLayerMask layerBitVal = (SceneLayerMask)(1UL << i);
            if ((camera.LayerMask & layerBitVal) == layerBitVal)
            {
                queue.SetMarker($"Skipped masked layer {i + 1}/{sceneData.Layers.Count} - {sceneData.Layers[i].Name}");
                continue;
            }

            queue.BeginEvent($"Render Layer {i+1}/{sceneData.Layers.Count} - {sceneData.Layers[i].Name}");
            context.Layer = sceneData.Layers[i];
            RenderChainLink stepRender = BuildRender(sceneData, context.Layer, camera);
            stepRender.Run(queue, camera, context, time);
            RenderChainLink.Recycle(stepRender);
            queue.EndEvent();
        }

        queue.BeginEvent($"Post-Render");
        RenderChainLink stepPostRender = BuildPostRender(sceneData, camera);
        stepPostRender.Run(queue, camera, context, time);
        RenderChainLink.Recycle(stepPostRender);
        queue.EndEvent();
        queue.End();
    }

    protected override void OnDispose(bool immediate)
    {
        // Dispose of render steps
        for (int i = 0; i < _stepList.Count; i++)
            _stepList[i].Dispose();
    }

    internal RenderService Renderer { get; }
}
