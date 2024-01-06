using Molten.Graphics;
using Molten.Input;

namespace Molten;

/// <summary>
/// Represents a layer or category of objects within a <see cref="Scene"/>, intended as an aid to order and organize objects.
/// </summary>
public class SceneLayer
{
    internal LayerRenderData Data { get; set; }

    internal List<SceneObject> Objects { get; }


    internal Dictionary<Type, ComponentTypeTracker> Trackers;

    /// <summary>
    /// Gets the layer's parent scene. This will only change (to null) in the event the layer is removed from it's parent scene.
    /// </summary>
    public Scene ParentScene { get; internal set; }

    internal SceneLayer()
    {
        Objects = new List<SceneObject>();
        Trackers = new Dictionary<Type, ComponentTypeTracker>();
        Track<IPickable<Vector2F>>();
        Track<IPickable<Vector3F>>();
        Track<IInputReceiver<KeyboardDevice>>();
        Track<IInputReceiver<MouseDevice>>();
        Track<IInputReceiver<GamepadDevice>>();
        Track<IInputReceiver<TouchDevice>>();
    }

    /// <summary>
    /// Starts tracking the add and removal of any <see cref="SceneComponent"/>s that derive/implement <typeparamref name="T"/>, for the current <see cref="SceneLayer"/>.
    /// <para>If <typeparamref name="T"/> is already tracked, the provided callbacks (if any) will still be added to it's tracker, in addition to any existing callbacks.</para>
    /// </summary>
    /// <typeparam name="T">The type to start tracking.</typeparam>
    /// <param name="addCallback">A callback to invoke each time a <typeparamref name="T"/> is added.</param>
    /// <param name="removeCallback">A callback to invoke each time a <typeparamref name="T"/> is removed.</param>
    /// <param name="includeExisting">If true, any existing components deriving or implementing <typeparamref name="T"/> will be added to the tracker, 
    /// if the type isn't already active.</param>
    public void Track<T>(Action<T> addCallback = null, Action<T> removeCallback = null, bool includeExisting = false)
        where T : class
    {
        Type t = typeof(T);
        if (!Trackers.TryGetValue(t, out ComponentTypeTracker tracker))
        {
            tracker = new ComponentTypeTracker<T>();
            Trackers[t] = tracker;

            // Scan for existing components of type T, if allowed.
            if (includeExisting)
            {
                foreach (SceneObject obj in Objects)
                {
                    foreach (SceneComponent comp in obj.Components)
                    {
                        if (t.IsAssignableFrom(comp.GetType()))
                            tracker.Add(comp);
                    }
                }
            }
        }

        ComponentTypeTracker<T> cTracker = tracker as ComponentTypeTracker<T>;
        cTracker.Unhook(addCallback, removeCallback);
    }

    /// <summary>
    /// Gets a <see cref="IReadOnlyList{T}"/> for the tracked type <typeparamref name="T"/>, 
    /// or null if <typeparamref name="T"/> is not tracked via <see cref="Track{T}(Action{T}, Action{T}, bool)"/>.
    /// </summary>
    /// <typeparam name="T">The tracked type list to be retrieved.</typeparam>
    /// <returns></returns>
    public IReadOnlyList<T> GetTracked<T>()
        where T : class
    {
        if (Trackers.TryGetValue(typeof(T), out ComponentTypeTracker tracker))
        {
            ComponentTypeTracker<T> cTracker = tracker as ComponentTypeTracker<T>;
            return cTracker.GetObjects();
        }

        return null;
    }

    /// <summary>
    /// Removes the specified callbacks from the tracker of <typeparamref name="T"/>, if any.
    /// </summary>
    /// <param name="addCallback">The add callback to be removed.</param>
    /// <param name="removeCallback">The remove callback to be removed.</param>
    public void RemoveTrackCallbacks<T>(Action<T> addCallback = null, Action<T> removeCallback = null)
        where T : class
    {
        if (Trackers.TryGetValue(typeof(T), out ComponentTypeTracker tracker))
        {
            ComponentTypeTracker<T> cTracker = tracker as ComponentTypeTracker<T>;
            cTracker.Unhook(addCallback, removeCallback);
        }            
    }

    /// <summary>
    /// Stops tracking the add and removal of any <see cref="SceneComponent"/>s that derive/implement <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to stop tracking.</typeparam>
    public void Untrack<T>()
        where T : class
    {
        Type t = typeof(T);
        if (Trackers.TryGetValue(t, out ComponentTypeTracker tracker))
        {
            Trackers.Remove(t);
            tracker.Clear();
        }
    }

    /// <summary>
    /// Adds a new object to the current <see cref="SceneLayer"/> with the specified <see cref="SceneComponent"/> attached to it.
    /// </summary>
    /// <typeparam name="C">The type of <see cref="SceneComponent"/>.</typeparam>
    /// <param name="flags">The object update flags.</param>
    /// <param name="visible">Whether or not the object is spawned visible.</param>
    /// <returns>The <see cref="SceneComponent"/> which was added to the new object. It's parent object can be retrieved via <see cref="SceneComponent.Object"/>.</returns>
    public C AddObjectWithComponent<C>(ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true) where C : SceneComponent, new()
    {
        return ParentScene.AddObjectWithComponent<C>(this, flags, visible);
    }

    /// <summary>
    /// Adds a new object to the current <see cref="SceneLayer"/> with the specified <see cref="SceneComponent"/> attached to it.
    /// </summary>
    /// <param name="componentTypes">A list the type of each <see cref="SceneComponent"/> to be added to the object.</param>
    /// <param name="flags">The object update flags.</param>
    /// <param name="visible">Whether or not the object is spawned visible.</param>
    /// <returns>The newly-created <see cref="SceneObject"/> containing all of the valid components that were specified.</returns>
    public SceneObject AddObjectWithComponents(IList<Type> componentTypes, ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true)
    {
        return ParentScene.AddObjectWithComponents(componentTypes, this, flags, visible);
    }

    /// <summary>
    /// Adds an object to the current <see cref="SceneLayer"/> in it's parent <see cref="Scene"/>.
    /// </summary>
    /// <param name="obj">The object to be added.</param>
    public void AddObject(SceneObject obj)
    {
        ParentScene.AddObject(obj, this);
    }

    /// <summary>
    /// Removes an object from the current <see cref="SceneLayer"/> in it's parent <see cref="Scene"/>.
    /// </summary>
    /// <param name="obj">The object to be removed.</param>
    public void RemoveObject(SceneObject obj)
    {
        ParentScene.RemoveObject(obj, this);
    }

    /// <summary>
    /// Brings the scene layer to the front, on top of all the parent scene's other layers.
    /// </summary>
    public void BringToFront()
    {
        ParentScene.QueueLayerReorder(this, ReorderMode.BringToFront);
    }

    /// <summary>
    /// Sends the current layer to the back, on behind of all the parent scene's other layers.
    /// </summary>
    public void SendToBack()
    {
        ParentScene.QueueLayerReorder(this, ReorderMode.SendToBack);
    }

    /// <summary>
    /// Brings the current layer forward by one layer, essentially swapping it's position/order with the layer in front of it.
    /// </summary>
    public void PushForward()
    {
        ParentScene.QueueLayerReorder(this, ReorderMode.PushForward);
    }

    /// <summary>
    /// Sends the current layer backward by one layer, essentially swapping it's position/order with the layer behind it.
    /// </summary>
    public void PushBackward()
    {
        ParentScene.QueueLayerReorder(this, ReorderMode.PushBackward);
    }

    /// <summary>
    /// Gets or sets whether objects in the current <see cref="SceneLayer"/> ignore raycast hits checks. The default value is false.
    /// </summary>
    public bool IgnoreRaycastHit { get; set; }

    /// <summary>
    /// Gets or sets the layer name
    /// </summary>
    public string Name { get; set; } // TODO implement handling to update the parent scene's dictionary key for the layer.

    /// <summary>
    /// Gets the ID of the layer.
    /// </summary>
    public int LayerID { get; internal set; }
}
