using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for updating and rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIManagerComponent : SpriteRenderComponent, IPickable<Vector2F>
    {       
        UIContainer _root;

        protected override void OnInitialize(SceneObject obj)
        {
            base.OnInitialize(obj);

            _root = new UIContainer()
            {
                LocalBounds = new Rectangle(0,0,600,480),
                Manager = this,
            };
            Children = _root.Children;
        }

        protected override void OnDispose()
        {

        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            _root.Update(time);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            _root.Render(sb);
        }

        public IPickable<Vector2F> Pick(Vector2F pos, Timing time)
        {
            return _root.Pick(pos, time);
        }

        public bool OnScrollWheel(InputScrollWheel wheel)
        {
            throw new NotImplementedException();
        }

        public void OnHover(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnEnter(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnLeave(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnPressed(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnDoublePressed(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnHeld(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnDragged(CameraInputTracker tracker)
        {
            throw new NotImplementedException();
        }

        public void OnReleased(CameraInputTracker tracker, bool releasedOutside)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Vector2F pos)
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            throw new NotImplementedException();
        }

        public void Unfocus()
        {
            throw new NotImplementedException();
        }

        /*public void PointerHeld(UIPointerTracker tracker)
        {
            if (tracker.Button == PointerButton.Left)
            {
                if (tracker.Pressed != null)
                {
                    if (tracker.Held == null && tracker.Pressed.Contains(tracker.Position))
                    {
                        tracker.Held = tracker.Pressed;
                        tracker.Held.OnHeld(tracker);
                    }
                }
            }
        }*/

        /// <summary>
        /// Gets all of the child <see cref="UIElement"/> attached to <see cref="Root"/>. This is an alias propety for <see cref="Root"/>.Children.
        /// </summary>
        public UIElementLayer Children { get; private set; }

        /// <summary>
        /// Gets the root <see cref="UIContainer"/>.
        /// </summary>
        public UIContainer Root => _root;

        public IPickable<Vector2F> ParentPickable => throw new NotImplementedException();

        public bool IsFocused { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
