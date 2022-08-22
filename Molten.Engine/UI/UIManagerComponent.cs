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

            public UIElement Held;

            public UIElement Dragging;

            public void Reset()
            {
                Pressed = null;
                Held = null;
                Dragging = null;
            }
        }

        CameraComponent _camera;
        UIContainer _root;
        Dictionary<ScenePointerTracker, UITracker> _trackers;

        public UIManagerComponent()
        {
            _trackers = new Dictionary<ScenePointerTracker, UITracker>();
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

        public void PointerDrag(ScenePointerTracker tracker)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (uiTracker.Pressed != null)
                {
                    if (uiTracker.Dragging == null)
                    {
                        if (uiTracker.Pressed.Contains(tracker.Position))
                        {
                            uiTracker.Dragging = uiTracker.Pressed;

                            // TODO perform start of drag-drop if element allows being drag-dropped
                        }
                    }

                    uiTracker.Dragging?.OnDragged(tracker);
                }
            });
        }

        public void PointerHeld(ScenePointerTracker tracker)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if(tracker.Button == PointerButton.Left)
                {
                    if(uiTracker.Pressed != null)
                    {
                        if(uiTracker.Held == null && uiTracker.Pressed.Contains(tracker.Position))
                        {
                            uiTracker.Held = uiTracker.Pressed;
                            uiTracker.Held.OnHeld(tracker);
                        }
                    }
                }
            });
        }

        public void PointerPressed(ScenePointerTracker tracker)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    if (uiTracker.Pressed == null)
                    {
                        uiTracker.Pressed = _root.Pick(tracker.Position);
                        if (uiTracker.Pressed != null)
                            uiTracker.Pressed.OnPressed(tracker);
                    }
                }
            });
        }

        public void PointerReleasedOutside(ScenePointerTracker tracker)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    uiTracker.Pressed?.OnReleased(tracker, true);
                    uiTracker.Reset();
                }
            });
        }

        public void PointerReleased(ScenePointerTracker tracker, bool wasDragged)
        {
            UpdateTracker(tracker, (uiTracker) =>
            {
                if (tracker.Button == PointerButton.Left)
                {
                    if (uiTracker.Pressed != null)
                    {
                        bool inside = uiTracker.Pressed.Contains(tracker.Position);
                        uiTracker.Pressed.OnReleased(tracker, !inside);

                        if(uiTracker.Dragging != null)
                        {
                            // TODO perform drop action of drag-drop, if element allows being drag-dropped and target can receive drag-drop actions.
                        }

                        uiTracker.Reset();
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
            UIElement prevHover = HoverElement; 
            Vector2F localPos;

            HoverElement = _root.Pick(pos);

            // Trigger on-leave of previous hover element.
            if (HoverElement != prevHover)
                prevHover?.OnLeave(pos);

            // Update currently-hovered element
            if (HoverElement != null)
            {
                localPos = pos - (Vector2F)HoverElement.GlobalBounds.TopLeft;
                if (prevHover != HoverElement)
                    HoverElement.OnEnter(pos);

                HoverElement.OnHover(localPos, pos);
            }
        }

        public void PointerFocus()
        {

        }

        public void PointerUnfocus()
        {

        }

        private void UpdateRootBounds(IInputCamera camera, IRenderSurface2D surface)
        {
            _root.LocalBounds = new Rectangle(0, 0, (int)surface.Width, (int)surface.Height);
        }

        /// <summary>
        /// Gets all of the child <see cref="UIElement"/> attached to <see cref="Root"/>. This is an alias propety for <see cref="Root"/>.Children.
        /// </summary>
        public UIChildCollection Children { get; private set; }

        /// <summary>
        /// Gets the root <see cref="UIContainer"/>.
        /// </summary>
        public UIContainer Root => _root;

        public string Tooltip => Name;

        /// <summary>
        /// The current <see cref="UIElement"/> being hovered over by a pointing device (e.g. mouse or stylus).
        /// </summary>
        public UIElement HoverElement { get; private set; }

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
