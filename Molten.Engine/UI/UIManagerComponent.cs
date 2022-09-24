using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for updating and rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIManagerComponent : SpriteRenderComponent, IPickable
    {
        public event SceneInputEventHandler<PointerButton> OnObjectFocused;
        public event SceneInputEventHandler<PointerButton> OnObjectUnfocused;

        public event ObjectHandler<UIElement> FocusedChanged;
        Dictionary<ulong, List<UIPointerTracker>> _trackers;
        PointerButton[] _pButtons;

        UIElement _focused;
        UIContainer _root;

        public UIManagerComponent()
        {
            _pButtons = ReflectionHelper.GetEnumValues<PointerButton>();
            _trackers = new Dictionary<ulong, List<UIPointerTracker>>();
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

            // Update all pointer trackers
            foreach (KeyValuePair<ulong, List<UIPointerTracker>> kv in _trackers)
            {
                for (int j = 0; j < kv.Value.Count; j++)
                    kv.Value[j].Update(this, time);
            }

            _root.Update(time);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            _root.Render(sb);
        }

        public bool Pick(PointingDevice pDevice, Timing time)
        {
            if (pDevice == null)
                return false;

            if (pDevice.IsConnected && pDevice.IsEnabled)
                TrackPointingDevice(pDevice);

            Vector2F pos = pDevice.Position;
            HoveredElement = _root.Pick(pos);

            if (pDevice is MouseDevice mouse)
            {
                UIElement prevHover = HoveredElement;
                Vector2F localPos;

                // Trigger on-leave of previous hover element.
                if (HoveredElement != prevHover)
                    prevHover?.OnLeave(pos);

                // Update currently-hovered element
                if (HoveredElement != null)
                {
                    localPos = pos - (Vector2F)HoveredElement.GlobalBounds.TopLeft;
                    if (prevHover != HoveredElement)
                        HoveredElement.OnEnter(pos);

                    HoveredElement.OnHover(localPos, pos);
                }

                // Handle scroll wheel event
                if (mouse.ScrollWheel.Delta != 0)
                {
                    // TODO pass mouse.ScrollWheel values to UIElement.OnScroll;
                }
            }

            return HoveredElement != null;
        }


        private void TrackPointingDevice(PointingDevice device)
        {
            if (_trackers.ContainsKey(device.EOID))
                return;

            if (device.IsDisposed || !device.IsConnected || !device.IsEnabled)
                return;

            List<UIPointerTracker> trackers = new List<UIPointerTracker>();
            _trackers.Add(device.EOID, trackers);

            device.OnDisposing += Device_OnDisposing;

            for (int setID = 0; setID < device.StateSetCount; setID++)
            {
                foreach (PointerButton button in _pButtons)
                {
                    if (button == PointerButton.None)
                        continue;

                    trackers.Add(new UIPointerTracker(device, setID, button));
                }
            }
        }

        private void UntrackPointingDevice(PointingDevice device)
        {
            if (_trackers.TryGetValue(device.EOID, out List<UIPointerTracker> trackers))
            {
                foreach (UIPointerTracker tracker in trackers)
                    tracker.Clear();

                _trackers.Remove(device.EOID);
            }
        }

        private void Device_OnDisposing(EngineObject o)
        {
            UntrackPointingDevice(o as PointingDevice);
        }

        public bool Contains(Vector2F point)
        {
            return _root.Pick(point) != null;
        }

        public void PointerDrag(UIPointerTracker tracker)
        {
            if (tracker.Pressed != null)
            {
                if (tracker.Dragging == null)
                {
                    if (tracker.Pressed.Contains(tracker.Position))
                    {
                        tracker.Dragging = tracker.Pressed;

                        // TODO perform start of drag-drop if element allows being drag-dropped
                    }
                }

                tracker.Dragging?.OnDragged(tracker);
            }
        }

        public void PointerHeld(UIPointerTracker tracker)
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
        }

        public void PointerPressed(UIPointerTracker tracker)
        {
            if (tracker.Pressed == null)
            {
                tracker.Pressed = _root.Pick(tracker.Position);

                if (tracker.Pressed != null)
                {
                    tracker.Pressed.Focus();
                    tracker.Pressed.OnPressed(tracker);
                }
            }
        }

        public void PointerReleasedOutside(UIPointerTracker tracker)
        {
            if (tracker.Button == PointerButton.Left)
            {
                tracker.Pressed?.OnReleased(tracker, true);
                tracker.Reset();
            }
        }

        public void PointerReleased(UIPointerTracker tracker, bool wasDragged)
        {
            if (tracker.Button == PointerButton.Left)
            {
                if (tracker.Pressed != null)
                {
                    bool inside = tracker.Pressed.Contains(tracker.Position);
                    tracker.Pressed.OnReleased(tracker, !inside);

                    if (tracker.Dragging != null)
                    {
                        // TODO perform drop action of drag-drop, if element allows being drag-dropped and target can receive drag-drop actions.
                    }

                    tracker.Reset();
                }
            }
        }


        public void PointerEnter(Vector2F pos)
        {

        }

        public void PointerLeave(Vector2F pos)
        {

        }

        public void PointerFocus()
        {

        }

        public void PointerUnfocus()
        {

        }

        /// <summary>
        /// Gets all of the child <see cref="UIElement"/> attached to <see cref="Root"/>. This is an alias propety for <see cref="Root"/>.Children.
        /// </summary>
        public UIElementLayer Children { get; private set; }

        /// <summary>
        /// Gets the root <see cref="UIContainer"/>.
        /// </summary>
        public UIContainer Root => _root;

        public string Tooltip => Name;

        /// <summary>
        /// Gets the current <see cref="UIElement"/> being hovered over by a pointing device (e.g. mouse or stylus).
        /// </summary>
        public UIElement HoveredElement { get; private set; }

        /// <summary>
        /// Gets the currently-focused <see cref="UIElement"/>.
        /// </summary>
        public UIElement FocusedElement
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    if (_focused != null)
                        _focused.IsFocused = false;

                    _focused = value;

                    if (_focused != null)
                        _focused.IsFocused = true;

                    FocusedChanged?.Invoke(_focused);
                }
            }
        }
    }
}
