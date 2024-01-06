using Molten.Graphics;

namespace Molten;

public class RenderableComponent : SceneComponent
{
    Renderable _renderable;
    bool _visible = true;
    bool _inScene = false;
    ObjectRenderData _data;

    protected override void OnInitialize(SceneObject obj)
    {
        _data = new ObjectRenderData();

        AddToScene(obj);
        obj.OnRemovedFromScene += Obj_OnRemovedFromScene;
        obj.OnAddedToScene += Obj_OnAddedToScene;

        base.OnInitialize(obj);
    }

    private void AddToScene(SceneObject obj)
    {
        if (_inScene || _renderable == null)
            return;

        // Add mesh to render data if possible.
        if (_visible && obj.Scene != null)
        {
            obj.Scene.RenderData.AddObject(_renderable, _data, obj.Layer.Data);
            _inScene = true;
        }
    }

    private void RemoveFromScene(SceneObject obj)
    {
        if (!_inScene)
            return;

        obj.Scene.RenderData.RemoveObject(_renderable, _data, obj.Layer.Data);
        _inScene = false;
    }

    protected internal override bool OnRemove(SceneObject obj)
    {
        obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
        obj.OnAddedToScene -= Obj_OnAddedToScene;
        RemoveFromScene(obj);

        _renderable = null;
        _visible = true;

        return base.OnRemove(obj);
    }

    private void Obj_OnAddedToScene(SceneObject obj, Scene scene, SceneLayer layer)
    {
        AddToScene(obj);
    }

    private void Obj_OnRemovedFromScene(SceneObject obj, Scene scene, SceneLayer layer)
    {
        RemoveFromScene(obj);
    }

    public override void OnUpdate(Timing time)
    {
        _data.TargetTransform = Object.Transform.Global;
    }

    protected override void OnDispose()
    {
        RemoveFromScene(Object);
    }

    /// <summary>The renderable object (e.g. a mesh or sprite) that should be drawn at the location of the component's parent object.</summary>
    public Renderable RenderedObject
    {
        get => _renderable;
        set
        {
            if (_renderable != value)
            {
                RemoveFromScene(Object);
                _renderable = value;
                AddToScene(Object);
            }
        }
    }

    public override bool IsVisible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;

                if (_visible)
                    AddToScene(Object);
                else
                    RemoveFromScene(Object);
            }
        }
    }
}
