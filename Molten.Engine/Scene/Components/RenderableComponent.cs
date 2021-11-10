using Molten.Graphics;

namespace Molten
{
    public abstract class RenderableComponent<T> : SceneComponent
        where T : class, IRenderable
    {
        protected T _renderable;
        protected bool _visible = true;
        protected bool _inScene = false;
        protected ObjectRenderData _data;

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
            if (!_inScene || _renderable == null)
                return;

            if (obj.Scene != null || _visible)
            {
                obj.Scene.RenderData.RemoveObject(_renderable, _data, obj.Layer.Data);
                _inScene = false;
            }
        }

        protected override void OnDestroy(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            // Reset State
            _renderable = null;
            _visible = true;

            base.OnDestroy(obj);
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

        /// <summary>The renderable object (e.g. a mesh or sprite) that should be drawn at the location of the component's parent object.</summary>
        public T RenderedObject
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

        /// <summary>
        /// Gets or sets the depth-write permission override for the current <see cref="SpriteRenderComponent"/>. <para/>
        /// If set to <see cref="GraphicsDepthWritePermission.Enabled"/>, the value provided by the current material will be used instead.
        /// To override the depth-write permission set by the current material, set this value to anything other than <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        public GraphicsDepthWritePermission DepthWriteOverride
        {
            get => _data.DepthWriteOverride;
            set => _data.DepthWriteOverride = value;
        }
    }
}
