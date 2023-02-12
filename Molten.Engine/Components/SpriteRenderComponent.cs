using Molten.Graphics;

namespace Molten
{
    public abstract class SpriteRenderComponent : SceneComponent
    {
        protected SpriteRenderer _spriteRenderer;
        protected bool _visible = true;
        protected bool _inScene = false;

        protected ObjectRenderData Data { get; private set; }

        protected override void OnInitialize(SceneObject obj)
        {
            Data = new ObjectRenderData();

            AddToScene(obj);
            obj.OnRemovedFromScene += Obj_OnRemovedFromScene;
            obj.OnAddedToScene += Obj_OnAddedToScene;

            if (obj.Engine.Renderer != null)
                _spriteRenderer = new SpriteRenderer(obj.Engine.Renderer.Device, OnRender);

            base.OnInitialize(obj);
        }

        private void AddToScene(SceneObject obj)
        {
            if (_inScene || _spriteRenderer == null || _spriteRenderer.Callback == null)
                return;

            // Add mesh to render data if possible.
            if (_visible && obj.Scene != null)
            {
                obj.Scene.RenderData.AddObject(_spriteRenderer, Data, obj.Layer.Data);
                _inScene = true;
            }
        }

        private void RemoveFromScene(SceneObject obj)
        {
            if (!_inScene || _spriteRenderer == null)
                return;

            if (obj.Scene != null || _visible)
            {
                obj.Scene.RenderData.RemoveObject(_spriteRenderer, Data, obj.Layer.Data);
                _inScene = false;
            }
        }

        protected internal override bool OnRemove(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            // Reset State
            _spriteRenderer = null;
            _visible = true;

            return base.OnRemove(obj);
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
            Data.TargetTransform = Object.Transform.Global;
        }

        protected abstract void OnRender(SpriteBatcher sb);

        /// <summary>
        /// Gets or sets whether the current component is visible.
        /// </summary>
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
    }
}
