using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Scene : EngineObject
    {
        SceneRenderData _data;

        internal List<SceneObject> Objects;
        internal List<ISprite> Sprites;
        internal  HashSet<IUpdatable> Updatables;

        ThreadedQueue<SceneChange> _pendingChanges;

        public Scene(string name, Engine engine)
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

        public void AddObject(SceneObject obj)
        {
            SceneAddObject change = SceneAddObject.Get();
            change.Object = obj;
            _pendingChanges.Enqueue(change);
        }

        public void RemoveObject(SceneObject obj)
        {
            SceneRemoveObject change = SceneRemoveObject.Get();
            change.Object = obj;
            _pendingChanges.Enqueue(change);
        }

        public void AddSprite(ISprite sprite)
        {
            SceneAddSprite change = SceneAddSprite.Get();
            change.Sprite = sprite;
            _pendingChanges.Enqueue(change);
        }

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

            // Update children.
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
        public Camera OutputCamera
        {
            get => _data.RenderCamera;
            set => _data.RenderCamera = value;
        }

        /// <summary>Gets the <see cref="Engine"/> instance that the <see cref="Scene"/> is bound to.</summary>
        public Engine Engine { get; private set; }

        public Color BackgroundColor
        {
            get => _data.BackgroundColor;
            set => _data.BackgroundColor = value;
        }
    }
}
