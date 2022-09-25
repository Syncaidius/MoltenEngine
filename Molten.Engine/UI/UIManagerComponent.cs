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

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            Rectangle constraintBounds;
            if (InputConstraintBounds.HasValue)
                constraintBounds = InputConstraintBounds.Value;
            else
                constraintBounds = _root.GlobalBounds;

            // Update all pointer trackers
            foreach (KeyValuePair<ulong, List<UIPointerTracker>> kv in _trackers)
            {
                for (int j = 0; j < kv.Value.Count; j++)
                    kv.Value[j].Update(time, ref constraintBounds);
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

            Vector2F pos = pDevice.Position;

            if (_root.Pick(pos) != null)
            {
                if (pDevice.IsConnected && pDevice.IsEnabled)
                    TrackPointingDevice(pDevice);

                return true;
            }

            return false;
        }


        private void TrackPointingDevice(PointingDevice device)
        {
            if (_trackers.ContainsKey(device.EOID))
                return;

            if (device.IsDisposed || !device.IsConnected || !device.IsEnabled)
                return;

            // Decide which constraint bounds to use.
            Rectangle constraintBounds;
            if (InputConstraintBounds.HasValue)
                constraintBounds = InputConstraintBounds.Value;
            else
                constraintBounds = _root.GlobalBounds;

            List<UIPointerTracker> trackers = new List<UIPointerTracker>();
            _trackers.Add(device.EOID, trackers);

            device.OnDisposing += Device_OnDisposing;

            for (int setID = 0; setID < device.StateSetCount; setID++)
            {
                foreach (PointerButton button in _pButtons)
                {
                    if (button == PointerButton.None)
                        continue;

                    trackers.Add(new UIPointerTracker(this, device, setID, button, ref constraintBounds));
                }
            }
        }

        private void UntrackPointingDevice(PointingDevice device)
        {
            if (_trackers.TryGetValue(device.EOID, out List<UIPointerTracker> trackers))
            {
                foreach (UIPointerTracker tracker in trackers)
                    tracker.Release();

                _trackers.Remove(device.EOID);
            }
        }

        private void Device_OnDisposing(EngineObject o)
        {
            UntrackPointingDevice(o as PointingDevice);
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

        public string Tooltip => Name;

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

        /// <summary>
        /// Gets or sets the constraint bounds for input. Only accepts input if it is within these bounds. 
        /// 
        /// <para>Any positional input data will be relative to the top-left of these bounds.</para>
        /// </summary>
        public Rectangle? InputConstraintBounds { get; set; }
    }
}
