using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for updating and rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIManagerComponent : SpriteRenderComponent, IPointerReceiver
    {
        UIElement _root;

        protected override void OnDispose()
        {
            
        }

        public void HandleInput(Vector2F inputPos)
        {
            // TODO Handle keyboard input/focusing here.
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

            Root.Render(sb);
        }

        public bool Contains(Vector2F point)
        {
            if (Root != null)
                return Root.Pick(point) != null;
            else
                return false;
        }

        public void PointerDrag(ScenePointerTracker button, Vector2F pos, Vector2F delta)
        {

        }

        public void PointerHeld(ScenePointerTracker button, Vector2F pos, Vector2F delta)
        {
            
        }

        public void PointerPressed(ScenePointerTracker button, Vector2F pos)
        {
            
        }

        public void PointerReleasedOutside(ScenePointerTracker button, Vector2F pos)
        {
            
        }

        public void PointerReleased(ScenePointerTracker button, Vector2F pos, bool wasDragged)
        {
            
        }

        public void PointerScroll(InputScrollWheel wheel)
        {
            
        }

        public void PointerEnter(Vector2F pos)
        {
            
        }

        public void PointerLeave(Vector2F pos)
        {
           
        }

        public void PointerHover(Vector2F pos)
        {
            if (Root != null)
                HoverElement = Root.Pick(pos);
            else
                HoverElement = null;
        }

        public void PointerFocus()
        {
            
        }

        public void PointerUnfocus()
        {
           
        }

        /// <summary>
        /// Gets or sets the Root <see cref="UIElement"/> to be drawn.
        /// </summary>
        public UIElement Root
        {
            get => _root;
            set
            {
                if (_root != value)
                {
                    // Un-set owner of current root UIElement, if any.
                    if (_root != null)
                        _root.Owner = null;

                    // Set new root UIElement and it's owner.
                    _root = value;
                    if (_root != null)
                    {
                        // A root element cannot have a parent.
                        if (_root.Parent != null)
                            _root.Parent.Children.Remove(_root);

                        _root.Owner = this;
                    }
                }
            }
        }

        public UIElement HoverElement { get; private set; }

        public string Tooltip => Name;
    }
}
