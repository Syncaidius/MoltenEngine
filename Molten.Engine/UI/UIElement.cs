using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public delegate void UIElementPointerHandler(UIElement element, ScenePointerTracker tracker);

    public delegate void UIElementHandler(UIElement element);

    public delegate void UIElementPositionHandler(UIElement element, Vector2F localPos, Vector2F globalPos);

    /// <summary>
    /// The base class for a UI component.
    /// </summary>
    [Serializable]
    public abstract class UIElement : EngineObject
    {
        public event UIElementPointerHandler Pressed;
        public event UIElementPointerHandler Released;

        public event UIElementPositionHandler Enter;
        public event UIElementPositionHandler Leave;

        /// <summary>
        /// Invoked when a pointer has hovered over the 
        /// </summary>
        public event UIElementPositionHandler Hovered;

        UIManagerComponent _manager;
        UIElement _parent;
        UITheme _theme;
        UIElementState _state;
        Rectangle _localBounds;
        Rectangle _globalBounds;
        Rectangle _borderBounds;
        Rectangle _renderBounds;

        public UIElement()
        {
            Children = new UIChildCollection(this);
            CompoundElements = new UIChildCollection(this);
            Engine = Engine.Current;
            State = UIElementState.Default;
            OnInitialize(Engine, Engine.Settings.UI);
            ApplyTheme();
        }

        protected virtual void OnInitialize(Engine engine, UISettings settings)
        {
            Margin.OnChanged += MarginPadding_OnChanged;
            Padding.OnChanged += MarginPadding_OnChanged;
        }

        private void MarginPadding_OnChanged()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (Parent != null)
            {
                _globalBounds = new Rectangle()
                {
                    X = Parent._renderBounds.X + _localBounds.X,
                    Y = Parent._renderBounds.Y + _localBounds.Y,
                    Width = _localBounds.Width,
                    Height = _localBounds.Height,
                };
            }
            else
            {
                _globalBounds = _localBounds;
            }

            _borderBounds = _globalBounds;
            _borderBounds.Inflate(-Margin.Left, -Margin.Top, -Margin.Right, -Margin.Bottom);

            _renderBounds = _borderBounds;
            _renderBounds.Inflate(-Padding.Left, -Padding.Top, -Padding.Right, -Padding.Bottom);

            OnUpdateCompoundBounds();
            foreach (UIElement e in CompoundElements)
                e.UpdateBounds();

            OnUpdateChildBounds();
            foreach (UIElement e in Children)
                e.UpdateBounds();

            OnUpdateBounds();
        }

        protected virtual void ApplyTheme()
        {
            if (_theme == null)
                return;

            foreach (UIElement e in CompoundElements)
                e.Theme = _theme;

            foreach (UIElement e in Children)
                e.Theme = _theme;

            _theme?.ApplyStyle(this);
            UpdateBounds();
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);

            for (int i = CompoundElements.Count - 1; i >= 0; i--)
                CompoundElements[i].Update(time);

            for (int i = Children.Count - 1; i >= 0; i--)
                Children[i].Update(time);
        }

        /// <summary>
        /// Checks if the current <see cref="UIElement"/> contains the given <see cref="Vector2F"/>. This does not test any child <see cref="UIElement"/> objects.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector2F point)
        {
            return _globalBounds.Contains(point);
        }

        /// <summary>
        /// Returns the current <see cref="UIElement"/> or one of it's children (recursive), depending on which contains <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point to use for picking a <see cref="UIElement"/>.</param>
        /// <param name="ignoreRules">If true, <see cref="InputRules"/> is ignored when picking.</param>
        /// <returns></returns>
        public UIElement Pick(Vector2F point, bool ignoreRules = false)
        {
            UIElement result = null;

            if (Contains(point))
            {
                if ((!ignoreRules && HasInputRules(UIInputRuleFlags.Compound)))
                {
                    for (int i = CompoundElements.Count - 1; i >= 0; i--)
                    {
                        result = CompoundElements[i].Pick(point);
                        if (result != null)
                            return result;
                    }
                }

                if ((!ignoreRules && HasInputRules(UIInputRuleFlags.Children)))
                {
                    for (int i = Children.Count - 1; i >= 0; i--)
                    {
                        result = Children[i].Pick(point);
                        if (result != null)
                            return result;
                    }
                }

                if(!ignoreRules && HasInputRules(UIInputRuleFlags.Self))
                    return OnPicked(point) ? this : null;
            }

            return result;
        }

        protected virtual bool OnPicked(Vector2F globalPos)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the given <see cref="UIInputRuleFlags"/> are set on the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public bool HasInputRules(UIInputRuleFlags rules)
        {
            return (InputRules & rules) == rules;
        }

        /// <summary>
        /// Invoked when a pointer is hovering over the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="localPos">The local position of the pointer, relative to the current <see cref="UIElement"/>.</param>
        /// <param name="globalPos">The global position of the pointer.</param>
        public virtual void OnHover(Vector2F localPos, Vector2F globalPos)
        {
            if (State == UIElementState.Default)
                State = UIElementState.Hovered;

            Hovered?.Invoke(this, localPos, globalPos);
        }

        /// <summary>
        /// Invoked when a pointer enters the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="localPos">The local position of the pointer, relative to the current <see cref="UIElement"/>.</param>
        /// <param name="globalPos">The global position of the pointer.</param>
        public virtual void OnEnter(Vector2F localPos, Vector2F globalPos)
        {
            Enter?.Invoke(this, localPos, globalPos);
        }

        /// <summary>
        /// Invoked when a pointer leaves the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="localPos">The local position of the pointer, relative to the current <see cref="UIElement"/>.</param>
        /// <param name="globalPos">The global position of the pointer.</param>
        public virtual void OnLeave(Vector2F localPos, Vector2F globalPos)
        {
            if (State == UIElementState.Hovered)
                State = UIElementState.Default;

            Leave?.Invoke(this, localPos, globalPos);
        }

        public virtual void OnPressed(ScenePointerTracker tracker)
        {
            if (State != UIElementState.Disabled && State != UIElementState.Pressed)
            {
                State = UIElementState.Pressed;
                Pressed?.Invoke(this, tracker);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="releasedOutside">The current <see cref="UIElement"/> was released outside of it's bounds.</param>
        public virtual void OnReleased(ScenePointerTracker tracker, bool releasedOutside)
        {
            if (State == UIElementState.Pressed)
            {
                State = UIElementState.Default;
                Released?.Invoke(this, tracker);
            }
        }

        protected override void OnDispose() { }

        protected virtual void OnUpdateBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of compound elements.
        /// </summary>
        protected virtual void OnUpdateCompoundBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of child elements.
        /// </summary>
        protected virtual void OnUpdateChildBounds() { }

        protected virtual void OnUpdate(Timing time) { }

        internal void Render(SpriteBatcher sb)
        {
            if (IsClipEnabled && sb.PushClip(GlobalBounds))
            {
                OnRenderSelf(sb);
                sb.PopClip();
            }
            else
            {
                OnRenderSelf(sb);
            }

            CompoundElements.Render(sb, ref _globalBounds);
            Children.Render(sb, ref _renderBounds);
        }

        protected virtual void OnRenderSelf(SpriteBatcher sb) { }

        /// <summary>
        /// Gets or sets the local bounds of the current <see cref="UIElement"/>.
        /// </summary>
        [DataMember]
        public Rectangle LocalBounds
        {
            get => _localBounds;
            set
            {
                _localBounds = value;
                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets the global bounds, relative to the <see cref="UIManagerComponent"/> that is drawing the current <see cref="UIElement"/>.
        /// <para>Global bounds are the area in which input is accepted and from which <see cref="RenderBounds"/> is calculated, based on padding, borders and other properties.</para>
        /// </summary>
        public Rectangle GlobalBounds => _globalBounds;

        /// <summary>
        /// Gets the bounds in which child components should be drawn.
        /// </summary>
        public Rectangle RenderBounds => _renderBounds;

        /// <summary>
        /// Gets the bounds at which borders should extend to beyond the <see cref="RenderBounds"/>.
        /// </summary>
        public Rectangle BorderBounds => _borderBounds;

        /// <summary>
        /// Gets or sets whether clipping is enabled.
        /// </summary>
        public bool IsClipEnabled { get; set; }

        /// <summary>
        /// Gets a read-only list of child components attached to the current <see cref="UIElement"/>.
        /// </summary>
        public UIChildCollection Children { get; }

        /// <summary>
        /// Gets a list of compound child <see cref="UIElement"/>. These cannot be externally-modified outside of the current <see cref="UIElement"/>.
        /// </summary>
        protected UIChildCollection CompoundElements { get; }

        /// <summary>
        /// Gets the parent of the current <see cref="UIElement"/>.
        /// </summary>
        public UIElement Parent
        {
            get => _parent;
            internal set
            {
                if (_parent != value)
                {
                    _parent = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Engine"/> instance that the current <see cref="UIElement"/> is bound to.
        /// </summary>
        public Engine Engine { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="UIManagerComponent"/> that will draw the current <see cref="UIElement"/>.
        /// </summary>
        internal UIManagerComponent Manager
        {
            get => _manager;
            set
            {
                if (_manager != value)
                {
                    _manager = value;
                    CompoundElements.SetManager(_manager);
                    Children.SetManager(_manager);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UITheme"/> that should be applied to the current <see cref="UIElement"/>. Themes provide a set of default appearance values and configuration.
        /// </summary>
        public UITheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;

                if (_theme != null)
                    ApplyTheme();
            }
        }

        /// <summary>
        /// Gets the <see cref="UIElementState"/> of the current <see cref="UIElement"/>.
        /// </summary>
        public UIElementState State
        {
            get => _state;
            set
            {
                if(_state != value)
                {
                    _state = value;
                    _theme?.ApplyStyle(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the input rules for the current <see cref="UIElement"/>.
        /// </summary>
        public UIInputRuleFlags InputRules { get; set; } = UIInputRuleFlags.All;

        /// <summary>
        /// Gets whether or not the current <see cref="UIElement"/> is a compound component of another element.
        /// </summary>
        public bool IsCompoundChild { get; internal set; }

        [DataMember]
        public UISpacing Margin { get; } = new UISpacing();

        [DataMember]
        public UISpacing Padding { get; } = new UISpacing();

        [DataMember]
        public UIAnchorFlags Anchor { get; set; }
    }
}
