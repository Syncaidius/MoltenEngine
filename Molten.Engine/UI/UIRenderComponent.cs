using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIRenderComponent : SpriteRenderComponent, ICursorAcceptor
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

        public ICursorAcceptor HandleInput(Vector2F inputPos)
        {
            return null;
        }

        public bool Contains(Vector2F point)
        {
            if (Root != null)
                return Root.Contains(point);
            else
                return false;
        }

        public void InvokeCursorClickStarted(Vector2F pos, MouseButton button)
        {
            
        }

        public void InvokeCursorClickCompletedOutside(Vector2F pos, MouseButton button)
        {
            
        }

        public void InvokeCursorClickCompleted(Vector2F pos, bool wasDragged, MouseButton button)
        {
            
        }

        public void InvokeCursorWheelScroll(InputScrollWheel wheel)
        {
            
        }

        public void InvokeCursorEnter(Vector2F pos)
        {
            
        }

        public void InvokeCursorLeave(Vector2F pos)
        {
           
        }

        public void InvokeCursorHover(Vector2F pos)
        {
            
        }

        public void InvokeCursorFocus()
        {
            
        }

        public void InvokeCursorDrag(Vector2F pos, Vector2F delta, MouseButton button)
        {
            
        }

        public void InvokeCursorUnfocus()
        {
           
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

        public string Tooltip => throw new NotImplementedException();
    }
}
