using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SceneLayer
    {
        internal LayerRenderData Data;
        internal List<ISceneObject> Objects;
        internal List<IRenderable2D> Renderables2d;
        internal HashSet<IUpdatable> Updatables;
        internal List<ICursorAcceptor> InputAcceptors;

        public Scene ParentScene { get; internal set; }

        internal SceneLayer()
        {
            Objects = new List<ISceneObject>();
            Renderables2d = new List<IRenderable2D>();
            Updatables = new HashSet<IUpdatable>();
            InputAcceptors = new List<ICursorAcceptor>();
        }

        /// <summary>
        /// Adds an object to the current <see cref="SceneLayer"/> in it's parent <see cref="Scene"/>.
        /// </summary>
        /// <param name="obj">The object to be added.</param>
        public void AddObject(ISceneObject obj)
        {
            ParentScene.AddObject(obj, this);
        }

        /// <summary>
        /// Removes an object from the current <see cref="SceneLayer"/> in it's parent <see cref="Scene"/>.
        /// </summary>
        /// <param name="obj">The object to be removed.</param>
        public void RemoveObject(ISceneObject obj)
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
