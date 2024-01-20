using Molten.Graphics;

namespace Molten;

public delegate void SceneObjectHandler(SceneObject obj);
public delegate void SceneObjectVisibilityHandler(SceneObject obj, bool visible);
public delegate void SceneObjectSceneHandler(SceneObject obj, Scene scene, SceneLayer layer);
public delegate void SceneObjectLayerHandler(SceneObject obj, SceneLayer oldLayer, SceneLayer newLayer);

public sealed class SceneObject : EngineObject
{
    Engine _engine;
    Scene _scene;
    SceneLayer _layer;

    // Transform-related variables
    SceneObjectTransform _transform;
    ObjectUpdateFlags _updateFlags;
    bool _visible;

    SceneComponentCollection _components;
    SceneCollection<SceneObject> _children;

    public event SceneObjectVisibilityHandler OnVisibilityChanged;
    public event SceneObjectHandler OnUpdateFlagsChanged;

    /// <summary>
    /// Invoked when the object is added to a scene.
    /// </summary>
    public event SceneObjectSceneHandler OnAddedToScene;

    /// <summary>
    /// Occus when the object is removed from a scene.
    /// </summary>
    public event SceneObjectSceneHandler OnRemovedFromScene;

    /// <summary>Creates a new instance of <see cref="SceneObject"/>.</summary>
    /// <param name="engine">The <see cref="Molten.Engine"/> instance that the object will be bound to.</param>
    /// <param name="updateFlags">The object's initial update flags.</param>
    /// <param name="visible">Decides whether the object is initially visible when spawned. The default value is true.</param>
    internal SceneObject(Engine engine, ObjectUpdateFlags updateFlags = ObjectUpdateFlags.Children | ObjectUpdateFlags.Self, bool visible = true)
    {
        _engine = engine;
        _components = new SceneComponentCollection(Engine.Log, this);
        _children = new SceneCollection<SceneObject>(this);
        _transform = new SceneObjectTransform(this);

        _children.OnAdded += _children_OnItemAdded;
        _children.OnRemoved += _children_OnItemRemoved;

        _components.OnAdd += _components_OnAdd;
        _components.OnAdded += _components_OnAdded;
        _components.OnRemove += _components_OnRemove;
        _components.OnRemoved += _components_OnRemoved;

        _updateFlags = updateFlags;
        IsVisible = visible;
    }

    private void _components_OnRemoved(SceneCollection<SceneComponent> collection, SceneComponent component)
    {
        component.OnRemoved(this);
    }

    private void _components_OnRemove(SceneCollection<SceneComponent> collection, SceneComponent component, 
        ref SceneCollection<SceneComponent>.EventData data)
    {
        data.Cancel = component.OnRemove(this);
    }

    private void _components_OnAdded(SceneCollection<SceneComponent> collection, SceneComponent component)
    {
        component.Initialize(this);
        component.OnAdded(this);
    }

    private void _components_OnAdd(SceneCollection<SceneComponent> collection, SceneComponent component, 
        ref SceneCollection<SceneComponent>.EventData data)
    {
        data.Cancel = component.OnAdd(this);
    }

    private void _children_OnItemRemoved(SceneCollection<SceneObject> collection, SceneObject item)
    {
        item.Parent = null;
    }

    private void _children_OnItemAdded(SceneCollection<SceneObject> collection, SceneObject item)
    {
        item.Parent = this;
    }

    protected override void OnDispose(bool immediate) { }

    internal void Update(Timing time)
    {
        // Update own transform.
        _transform.Update(time);

        if ((_updateFlags & ObjectUpdateFlags.Self) == ObjectUpdateFlags.Self)
        {
            _components.Objects.For(0, (index, component) =>
            {
                if (component.IsEnabled)
                    component.OnUpdate(time);
            });
        }

        // Re-populate the local child list used when updating them.
        if ((_updateFlags & ObjectUpdateFlags.Children) == ObjectUpdateFlags.Children)
            _children.Objects.For(0, (index, child) => child.Update(time));

        _transform.ResetFlags();
    }

    /// <summary>Gets or sets whether the object is rendered. The object will still be updated if <see cref="UpdateFlags"/> is true.</summary>
    public bool IsVisible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                OnVisibilityChanged?.Invoke(this, _visible);
            }
        }
    }

    /// <summary>Gets or sets how the object will be updated. The default set flags are: <see cref="ObjectUpdateFlags.Self"/> and <see cref="ObjectUpdateFlags.Children"/></summary>
    public ObjectUpdateFlags UpdateFlags
    {
        get => _updateFlags;
        set
        {
            if (_updateFlags != value)
            {
                _updateFlags = value;
                OnUpdateFlagsChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the scene that the object is currently attached to, or null if not attached to any scenes.
    /// </summary>
    public Scene Scene => _scene;

    /// <summary>
    /// Gets the scene layer that the object is part of.
    /// </summary>
    public SceneLayer Layer
    {
        get => _layer;
        internal set
        {
            if (_layer != value)
            {
                if (_scene != null)
                {
                    OnRemovedFromScene?.Invoke(this, _scene, _layer);

                    for (int i = _components.Count - 1; i >= 0; i--)
                        _components[i].UnregisterFromLayer();
                }

                _layer = value;
                if (value != null)
                {
                    _scene = value.ParentScene;
                    OnAddedToScene?.Invoke(this, value.ParentScene, _layer);

                    for (int i = _components.Count - 1; i >= 0; i--)
                        _components[i].RegisterOnLayer();
                }
                else
                {
                    _scene = null;
                }

                for (int i = _children.Count - 1; i >= 0; i--)
                    _children[i].Layer = value;
            }
        }
    }

    /// <summary>
    /// Gets the object's <see cref="SceneObjectTransform"/>, which contains information about its position, rotation and scale and various helper properties.
    /// </summary>
    public SceneObjectTransform Transform => _transform;

    /// <summary>Gets a collection containing all of the child <see cref="SceneObject"/> instances attached to the current <see cref="SceneObject"/>.</summary>
    public SceneCollection<SceneObject> Children => _children;

    /// <summary>
    /// Gets a collection containing all of the <see cref="SceneComponent"/> objects attached to the current <see cref="SceneObject"/>.
    /// </summary>
    public SceneComponentCollection Components => _components;

    /// <summary>Gets the object's parent <see cref="SceneObject"/>, if any. Value is null if the current object has no parent.</summary>
    public SceneObject Parent { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Engine"/> instance that the scene object is bound to.
    /// </summary>
    public Engine Engine => _engine;

    /// <summary>
    /// Gets or sets whether or not the current object should be updated.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
