using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIRenderComponent : SpriteRenderComponent
    {
        UIComponent _root;
        ThreadedQueue<IUIChange> _pendingChanges = new ThreadedQueue<IUIChange>();

        protected override void OnDispose()
        {
            
        }

        internal void QueueChange(IUIChange change)
        {
            _pendingChanges.Enqueue(change);
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Root == null)
                return;

            Root.Update(time);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            if (Root == null)
                return;

            IUIChange change;
            while (_pendingChanges.TryDequeue(out change))
                change.Process();

            Root.Render(sb);
        }

        /// <summary>
        /// Gets or sets the Root <see cref="UIComponent"/> to be drawn.
        /// </summary>
        public UIComponent Root
        {
            get => _root;
            set
            {
                if (_root != value)
                {
                    if (_root != null)
                    {
                        _root.Root = null;
                        _root.RenderComponent = null;
                    }

                    _root = value;

                    if (_root != null)
                    {
                        _root.RenderComponent = this;
                        _root.Root = _root;
                    }
                }
            }
        }

    }
}
