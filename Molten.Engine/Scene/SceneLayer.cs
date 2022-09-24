using Molten.Graphics;

namespace Molten
{
    /// <summary>
    /// Represents a layer or category of objects within a <see cref="Scene"/>, intended as an aid to order and organize objects.
    /// </summary>
    public class SceneLayer
    {
        internal LayerRenderData Data { get; set; }

        internal List<SceneObject> Objects { get; }

        internal List<IPickable> Pickables { get; }

        internal List<IInputHandler> InputHandlers { get; }

        /// <summary>
        /// Gets the layer's parent scene. This will only change (to null) in the event the layer is removed from it's parent scene.
        /// </summary>
        public Scene ParentScene { get; internal set; }

        internal SceneLayer()
        {
            Objects = new List<SceneObject>();
            Pickables = new List<IPickable>();
            InputHandlers = new List<IInputHandler>();
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
}
