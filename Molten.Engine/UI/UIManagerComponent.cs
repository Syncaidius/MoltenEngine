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
        class UITracker
        {
            public UIElement Pressed;
        }

        CameraComponent _camera;
        UITheme _theme;
        UIContainer _root;
        Dictionary<ScenePointerTracker, UITracker> _trackers;
        List<ScenePointerTracker> _trackersToRemove;

        public UIManagerComponent()
        {
            _trackers = new Dictionary<ScenePointerTracker, UITracker>();
            _trackersToRemove = new List<ScenePointerTracker>();
        }

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

        private void UpdateTracker(ScenePointerTracker pTracker, Action<UITracker> callback)
        {
            if (!_trackers.TryGetValue(pTracker, out UITracker uiTracker))
            {
                uiTracker = new UITracker();
                _trackers.Add(pTracker, uiTracker);
            }

            callback.Invoke(uiTracker);
        }

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
            _root.Update(time);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            _root.Render(sb);
        }

        public bool Contains(Vector2F point)
        {
            return _root.Pick(point) != null;
        }

        public void PointerDrag(ScenePointerTracker tracker, Vector2F pos, Vector2F delta)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {

            });
        }

        public void PointerHeld(ScenePointerTracker tracker, Vector2F pos, Vector2F delta)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {

            });
        }

        public void PointerPressed(ScenePointerTracker tracker, Vector2F pos)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    if (uiTracker.Pressed == null)
                    {
                        uiTracker.Pressed = _root.Pick(pos);
                        if (uiTracker.Pressed != null)
                            uiTracker.Pressed.OnPressed(tracker);
                    }
                }
            });
        }

        public void PointerReleasedOutside(ScenePointerTracker tracker, Vector2F pos)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    uiTracker.Pressed?.OnReleased(tracker, true);
                    uiTracker.Pressed = null;
                }
            });
        }

        public void PointerReleased(ScenePointerTracker tracker, Vector2F pos, bool wasDragged)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    if (uiTracker.Pressed != null)
                    {
                        bool inside = uiTracker.Pressed.Contains(pos);
                        uiTracker.Pressed.OnReleased(tracker, !inside);
                        uiTracker.Pressed = null;
                    }
                }
            });
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
            HoverElement = _root.Pick(pos);
        }

        public void PointerFocus()
        {

        }

        public void PointerUnfocus()
        {

        }

        /// <summary>
        /// Gets all of the child <see cref="UIElement"/> attached to the current <see cref="UIManagerComponent"/>.
        /// </summary>
        public UIChildCollection Children { get; private set; }

        public string Tooltip => Name;

        /// <summary>
        /// The current <see cref="UIElement"/> being hovered over by a pointing device (e.g. mouse or stylus).
        /// </summary>
        public UIElement HoverElement { get; private set; }

        private void ApplyTheme(UITheme theme)
        {
            _root.Theme = theme;
        }

        private void UpdateRootBounds(IInputCamera camera, IRenderSurface2D surface)
        {
            _root.LocalBounds = new Rectangle(0, 0, (int)surface.Width, (int)surface.Height);
        }

        public CameraComponent Camera
        {
            get { return _camera; }
            set
            {
                if(_camera != value)
                {
                    if (_camera != null)
                    {
                        _camera.OnSurfaceChanged -= UpdateRootBounds;
                        _camera.OnSurfaceResized -= UpdateRootBounds;
                    }

                    _camera = value;

                    if (_camera != null)
                    {
                        UpdateRootBounds(_camera, _camera.OutputSurface);
                        _camera.OnSurfaceChanged += UpdateRootBounds;
                        _camera.OnSurfaceResized += UpdateRootBounds;
                    }
                }
            }
        }
    }
}
