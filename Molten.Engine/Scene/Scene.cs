using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>Manages and controls a scene composed of 3D and 2D objects, while also feeding the needed data to a renderer if available.</summary>
    public class Scene : EngineObject
    {
        SceneRenderData _data;

        internal List<SceneObject> Objects;
        internal List<ISprite> Sprites;
        internal  HashSet<IUpdatable> Updatables;

        ThreadedQueue<SceneChange> _pendingChanges;

        /// <summary>Creates a new instance of <see cref="Scene"/></summary>
        /// <param name="name">The name of the scene.</param>
        /// <param name="engine">The engine instance to which the scene will be bound.</param>
        internal Scene(string name, Engine engine)
        {
            Name = name;
            Engine = engine;
            _data = engine.Renderer.CreateRenderData();
            engine.AddScene(this);
            Objects = new List<SceneObject>();
            Sprites = new List<ISprite>();
            Updatables = new HashSet<IUpdatable>();
            _pendingChanges = new ThreadedQueue<SceneChange>();

            engine.Log.WriteLine($"Created scene '{name}'");
        }

        /// <summary>Adds a <see cref="SceneObject"/> to the scene.</summary>
        /// <param name="obj">The object to be added.</param>
        public void AddObject(SceneObject obj)
        {
            SceneAddObject change = SceneAddObject.Get();
            change.Object = obj;
            _pendingChanges.Enqueue(change);
        }

        /// <summary>Removes a <see cref="SceneObject"/> from the scene.</summary>
        /// <param name="obj">The object to be removed.</param>
        public void RemoveObject(SceneObject obj)
        {
            SceneRemoveObject change = SceneRemoveObject.Get();
            change.Object = obj;
            _pendingChanges.Enqueue(change);
        }

        /// <summary>Adds a sprite to the scene. This is a deferred action which will be performed on the scene's next update.</summary>
        /// <param name="sprite">The sprite to be added.</param>
        /// <param name="layer">The layer to which the sprite should be added.</param>
        public void AddSprite(ISprite sprite, int layer = 0)
        {
            SceneAddSprite change = SceneAddSprite.Get();
            change.Sprite = sprite;
            change.Layer = layer;
            _pendingChanges.Enqueue(change);
        }

        /// <summary>Removes a sprite from the scene. This is a deferred action which will be performed on the scene's next update.</summary>
        /// <param name="sprite">The sprite to be removed.</param>
        /// <param name="layer">The layer from which the sprite should be removed.</param>
        public void RemoveSprite(ISprite sprite, int layer = 0)
        {
            SceneRemoveSprite change = SceneRemoveSprite.Get();
            change.Sprite = sprite;
            change.Layer = layer;
            _pendingChanges.Enqueue(change);
        }

        public void Update(Timing time)
        {
            while (_pendingChanges.TryDequeue(out SceneChange change))
                change.Process(this);

            // Update root objects - Updated separately because it's safer/faster to assume at least 1 child object may need updating.
            foreach(SceneObject obj in Objects)
                obj.Update(time);

            // Updatable sprites
            foreach (IUpdatable up in Updatables)
                up.Update(time);
        }

        protected override void OnDispose()
        {
            Engine.RemoveScene(this);
            Engine.Renderer.DestroyRenderData(_data);
            Engine.Log.WriteLine($"Destroyed scene '{Name}'");
            base.OnDispose();
        }

        /// <summary>Gets or sets whether or not the scene is updated.</summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>Gets or sets whether the scene is rendered.</summary>
        public virtual bool IsVisible
        {
            get => _data.IsVisible;
            set => _data.IsVisible = value;
        }

        /// <summary>Gets the name of the scene.</summary>
        public string Name { get; private set; }

        /// <summary>Gets rendering information about the scene. Although some members of this object can be modified, please do not modify them outside of a renderer. 
        /// Doing so will cause unexpected behaviour.</summary>
        public SceneRenderData RenderData => _data;

        /// <summary>Gets or sets the scene's out camera. This acts as an eye when rendering the scene, allowing it to be viewed from the perspective of the camera.
        /// Scenes without a camera are rendered from a default view that is positioned at 0,0,5 and facing 0,0,0.</summary>
        public ICamera OutputCamera
        {
            get => _data.RenderCamera;
            set => _data.RenderCamera = value;
        }

        /// <summary>Gets or sets the scene's sprite camera. This is used when rendering <see cref="ISprite"/> objects over the 3D scene.
        /// If no sprite camera is set, the scene's output surface will be used as the default view.</summary>
        public ICamera SpriteCamera
        {
            get => _data.SpriteCamera;
            set => _data.SpriteCamera = value;
        }

        /// <summary>Gets the <see cref="Engine"/> instance that the <see cref="Scene"/> is bound to.</summary>
        public Engine Engine { get; private set; }

        /// <summary>Gets or sets the background color of the scene.</summary>
        public Color BackgroundColor
        {
            get => _data.BackgroundColor;
            set => _data.BackgroundColor = value;
        }
    }
}
