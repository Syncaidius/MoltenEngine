namespace Molten.Graphics;

public delegate void SceneRenderDataHandler(RenderService renderer, SceneRenderData data);

/// <summary>
/// A class for storing renderer-specific information about a scene.
/// </summary>
public class SceneRenderData
{
    /// <summary>
    /// Occurs just before the scene is about to be rendered.
    /// </summary>
    public event SceneRenderDataHandler OnPreRender;

    /// <summary>
    /// Occurs just after the scene has been rendered.
    /// </summary>
    public event SceneRenderDataHandler OnPostRender;

    /// <summary>
    /// If true, the scene will be rendered.
    /// </summary>
    public bool IsVisible = true;

    /// <summary>
    /// The ambient light color.
    /// </summary>
    public Color AmbientLightColor = Color.Black;

    public List<LayerRenderData> Layers = new List<LayerRenderData>();

    GpuTaskManager _taskManager;

    internal SceneRenderData(GpuTaskManager taskManager)
    {
        _taskManager = taskManager;
    }

    public void AddLayer(LayerRenderData data)
    {
        RenderLayerAdd task = _taskManager.Get<RenderLayerAdd>();
        task.LayerData = data;
        task.SceneData = this;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void RemoveLayer(LayerRenderData data)
    {
        RenderLayerRemove task = _taskManager.Get<RenderLayerRemove>();
        task.LayerData = data;
        task.SceneData = this;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void ReorderLayer(LayerRenderData data, ReorderMode mode)
    {
        RenderLayerReorder task = _taskManager.Get<RenderLayerReorder>();
        task.LayerData = data;
        task.SceneData = this;
        task.Mode = mode;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void AddObject(RenderCamera obj)
    {
        AddCamera task = _taskManager.Get<AddCamera>();
        task.Camera = obj;
        task.Data = this;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void RemoveObject(RenderCamera obj)
    {
        RemoveCamera task = _taskManager.Get<RemoveCamera>();
        task.Camera = obj;
        task.Data = this;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void AddObject(Renderable obj, ObjectRenderData renderData, LayerRenderData layer)
    {
        RenderableAdd task = _taskManager.Get<RenderableAdd>();
        task.Renderable = obj;
        task.Data = renderData;
        task.LayerData = layer;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    public void RemoveObject(Renderable obj, ObjectRenderData renderData, LayerRenderData layer)
    {
        RenderableRemove task = _taskManager.Get<RenderableRemove>();
        task.Renderable = obj;
        task.Data = renderData;
        task.LayerData = layer;
        _taskManager.Push(GpuPriority.StartOfFrame, task);
    }

    /// <summary>
    /// Invokes <see cref="OnPreRender"/> event.
    /// </summary>
    public void PreRenderInvoke(RenderService renderer) => OnPreRender?.Invoke(renderer, this);

    /// <summary>
    /// Invokes <see cref="OnPostRender"/> event.
    /// </summary>
    public void PostRenderInvoke(RenderService renderer) => OnPostRender?.Invoke(renderer, this);

    /* TODO:
    *  - Edit PointLights and CapsuleLights.Data directly in light scene components (e.g. PointLightComponent).
    *  - Renderer will upload the latest data to the GPU 
    */

    public LightList PointLights { get; } = new LightList(100, 100);

    public LightList CapsuleLights { get; } = new LightList(50, 100);

    public List<RenderCamera> Cameras { get; } = new List<RenderCamera>();

    /// <summary>
    /// Gets or sets the skybox cube-map texture.
    /// </summary>
    public ITextureCube SkyboxTexture { get; set; }
}
