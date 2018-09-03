using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class SceneComponent
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

        public virtual void OnRender(RenderEngine renderer, Timing time)
        {
            // for components that need to render, they will update their SceneRenderProxy instance with information about their mesh, effect/material and transform.
        }

        /// <summary>Gets the <see cref="SceneObject"/> that the component is attached to.</summary>
        public SceneObject Object { get; private set; }

        public virtual bool IsVisible { get; set; } = true;

        public virtual bool IsEnabled { get; set; } = true;
    }
}
