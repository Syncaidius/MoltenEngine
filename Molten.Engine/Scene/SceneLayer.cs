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
        internal SceneLayerData Data;
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
        /// Gets or sets whether objects in the current <see cref="SceneLayer"/> ignore raycast hits checks. The default value is false.
        /// </summary>
        public bool IgnoreRaycastHit { get; set; }

        /// <summary>
        /// Gets or sets the layer name
        /// </summary>
        public string Name { get; set; } // TODO implement handling to update the parent scene's dictionary key for the layer.
    }
}
