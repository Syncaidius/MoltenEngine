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

        UITheme _theme;
        UIElement _root;
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

            SettingValue<UITheme> themeSetting = obj.Engine.Settings.UI.Theme;
            Theme = themeSetting;
            themeSetting.OnChanged += ThemeSetting_OnChanged;
        }

        private void ThemeSetting_OnChanged(UITheme oldValue, UITheme newValue)
        {
            Theme = newValue;
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
                        uiTracker.Pressed = Root.Pick(pos);
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
                        _root.Manager = null;

                    // Set new root UIElement and it's owner.
                    _root = value;
                    if (_root != null)
                    {
                        // A root element cannot have a parent.
                        if (_root.Parent != null)
                            _root.Parent.Children.Remove(_root);

                        _root.Manager = this;
                        if (_theme.IsLoaded)
                            ApplyTheme(_theme);
                    }
                }
            }
        }

        public string Tooltip => Name;

        public UIElement HoverElement { get; private set; }

        public UITheme Theme
        {
            get => _theme;
            set
            {
                if(_theme != value)
                {
                    if(_theme != null)
                        _theme.OnContentLoaded -= ApplyTheme;

                    _theme = value ?? Engine.Current.Settings.UI.Theme;
                    if (_theme != null)
                        _theme.OnContentLoaded += ApplyTheme;

                    if (_theme.IsLoaded)
                        ApplyTheme(_theme);
                }
            }
        }

        private void ApplyTheme(UITheme theme)
        {
            if (_root != null)
                _root.Theme = theme;
        }
    }
}
