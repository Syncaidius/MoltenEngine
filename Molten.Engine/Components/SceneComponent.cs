namespace Molten;

public abstract class SceneComponent : EngineObject
{
    internal void Initialize(SceneObject obj)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        Object = obj;
        OnInitialize(obj);
    }
    internal void RegisterOnLayer()
    {
        Type t = GetType();
        foreach(KeyValuePair<Type, ComponentTypeTracker> p in Object.Layer.Trackers)
        {
            if (p.Key.IsAssignableFrom(t))
                p.Value.Add(this);
        }
    }

    internal void UnregisterFromLayer()
    {
        Type t = GetType();
        foreach (KeyValuePair<Type, ComponentTypeTracker> p in Object.Layer.Trackers)
        {
            if (p.Key.IsAssignableFrom(t))
                p.Value.Remove(this);
        }
    }

    /// <summary>
    /// Invoked just before the object is added to a parent.
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void OnInitialize(SceneObject obj) { }

    /// <summary>
    /// Invoked when the scene component is about to be added to a new parent. If false is returned, the object will not be added to <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">The <see cref="SceneObject"/> that will become the new parent.</param>
    protected internal virtual bool OnAdd(SceneObject obj) { return false; }

    /// <summary>
    /// Invoked when the scene component has been added to a new parent.
    /// </summary>
    /// <param name="obj">The <see cref="SceneObject"/> that has become the new parent.</param>
    protected internal virtual void OnAdded(SceneObject obj) { }

    /// <summary>
    /// Invoked when the scene component is about to be removed from its parent. If false is returned, the object will not be removed from <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">The <see cref="SceneObject"/> that will be removed as a parent.</param>
    protected internal virtual bool OnRemove(SceneObject obj) { return false; }

    /// <summary>
    /// Invoked when the scene component has been removed from its parent.
    /// </summary>
    /// <param name="obj">The <see cref="SceneObject"/> that was removed as a parent.</param>
    protected internal virtual void OnRemoved(SceneObject obj) { }

    /// <summary>
    /// Invoked when the scene component is due to be updated.
    /// </summary>
    /// <param name="time"></param>
    public virtual void OnUpdate(Timing time) { }

    /// <summary>Gets the <see cref="SceneObject"/> that the component is attached to.</summary>
    public SceneObject Object { get; private set; }

    /// <summary>
    /// Gets or sets whether the current <see cref="SceneComponent"/> should be visible.
    /// </summary>
    public virtual bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the current <see cref="SceneComponent"/> is enabled. If false, it will not receive <see cref="OnUpdate(Timing)"/> calls.
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets whether the current <see cref="SceneComponent"/> has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }
}
