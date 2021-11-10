namespace Molten
{
    public abstract class SceneComponent : IdentifiedObject
    {
        internal void Initialize(SceneObject obj)
        {
            Object = obj;
            OnInitialize(obj);
        }

        internal void Destroy(SceneObject obj)
        {
            OnDestroy(obj);
        }

        protected virtual void OnInitialize(SceneObject obj) { }

        protected virtual void OnDestroy(SceneObject obj) { }

        public virtual void OnUpdate(Timing time) { }

        /// <summary>Gets the <see cref="SceneObject"/> that the component is attached to.</summary>
        public SceneObject Object { get; private set; }

        public virtual bool IsVisible { get; set; } = true;

        public virtual bool IsEnabled { get; set; } = true;
    }
}
