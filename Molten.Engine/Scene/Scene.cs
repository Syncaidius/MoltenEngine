using Molten.Collections;
using Molten.Graphics;
using Molten.UI;
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
        internal SceneRenderData RenderData;
        internal List<SceneObject> Objects;
        internal List<ISprite> Sprites;
        internal HashSet<IUpdatable> Updatables;

        ThreadedQueue<SceneChange> _pendingChanges;
        UISystem _ui;

        /// <summary>Creates a new instance of <see cref="Scene"/></summary>
        /// <param name="name">The name of the scene.</param>
        /// <param name="engine">The engine instance to which the scene will be bound.</param>
        internal Scene(string name, Engine engine, SceneRenderFlags flags)
        {
            Name = name;
            Engine = engine;
            RenderData = engine.Renderer.CreateRenderData();
            RenderData.Flags = flags;

            engine.AddScene(this);
            Objects = new List<SceneObject>();
            Sprites = new List<ISprite>();
            Updatables = new HashSet<IUpdatable>();
            _pendingChanges = new ThreadedQueue<SceneChange>();
            _ui = new UISystem(this, engine);

            engine.Log.WriteLine($"Created scene '{name}'");
        }

        public void BringToFront()
        {
            Engine.Renderer?.BringToFront(RenderData);
        }

        public void SendToBack()
        {
            Engine.Renderer?.SendToBack(RenderData);
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
        public void AddSprite(ISprite sprite)
        {
            SceneAddSprite change = SceneAddSprite.Get();
            change.Sprite = sprite;
            _pendingChanges.Enqueue(change);
        }

        /// <summary>Removes a sprite from the scene. This is a deferred action which will be performed on the scene's next update.</summary>
        /// <param name="sprite">The sprite to be removed.</param>
        /// <param name="layer">The layer from which the sprite should be removed.</param>
        public void RemoveSprite(ISprite sprite)
        {
            SceneRemoveSprite change = SceneRemoveSprite.Get();
            change.Sprite = sprite;
            _pendingChanges.Enqueue(change);
        }

        public void Update(Timing time)
        {
            while (_pendingChanges.TryDequeue(out SceneChange change))
                change.Process(this);

            foreach (IUpdatable up in Updatables)
                up.Update(time);
        }

        protected override void OnDispose()
        {
            Engine.RemoveScene(this);
            Engine.Renderer.DestroyRenderData(RenderData);
            Engine.Log.WriteLine($"Destroyed scene '{Name}'");
            base.OnDispose();
        }

        /// <summary>Gets or sets whether or not the scene is updated.</summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>Gets or sets whether the scene is rendered.</summary>
        public virtual bool IsVisible
        {
            get => RenderData.IsVisible;
            set => RenderData.IsVisible = value;
        }

        /// <summary>Gets the name of the scene.</summary>
        public string Name { get; private set; }

        /// <summary>Gets or sets the scene's out camera. This acts as an eye when rendering the scene, allowing it to be viewed from the perspective of the camera.
        /// Scenes without a camera will not be updated or rendered.</summary>
        public ICamera OutputCamera
        {
            get => RenderData.Camera;
            set => RenderData.Camera = value;
        }

        /// <summary>Gets the <see cref="Engine"/> instance that the <see cref="Scene"/> is bound to.</summary>
        public Engine Engine { get; private set; }

        /// <summary>Gets or sets the background color of the scene.</summary>
        public Color BackgroundColor
        {
            get => RenderData.BackgroundColor;
            set => RenderData.BackgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the ambient light color of the scene.
        /// </summary>
        public Color AmbientColor
        {
            get => RenderData.AmbientLightColor;
            set => RenderData.AmbientLightColor = value;
        }

        /// <summary>
        /// Gets or sets the scene's render flags.
        /// </summary>
        public SceneRenderFlags RenderFlags
        {
            get => RenderData.Flags;
            set => RenderData.Flags = value;
        }

        /// <summary>Gets the <see cref="UISystem"/> bound to the current <see cref="Scene"/> instance.</summary>
        internal UISystem UI => _ui;

        /// <summary>
        /// Gets the scene's debug overlay. 
        /// The overlay can be added to another scene as an <see cref="ISprite"/> object if you want to render the overlay into a different scene.
        /// </summary>
        public ISceneDebugOverlay DebugOverlay => RenderData.DebugOverlay;
    }
}
