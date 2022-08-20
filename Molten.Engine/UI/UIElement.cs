using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public delegate void UIElementPointerHandler(UIElement element, ScenePointerTracker tracker);

    public delegate void UIElementHandler(UIElement element);

    public delegate void UIElementHandler<T>(T element) where T : UIElement;

    public delegate void UIElementPositionHandler(UIElement element, Vector2F localPos, Vector2F globalPos);

    public delegate void UIElementDeltaPositionHandler(UIElement element, ScenePointerTracker tracker, Vector2F localPos, Vector2F globalPos, Vector2I delta);

    public delegate void UIElementCancelHandler<T>(T element, UICancelEventArgs args) where T : UIElement;

    public delegate void UIParentChangedHandler(UIElement oldParent, UIElement newParent);

    public delegate void UIManagerChangedHandler(UIManagerComponent oldManager, UIManagerComponent newManager);

    /// <summary>
    /// The base class for a UI component.
    /// </summary>
    [Serializable]
    public abstract class UIElement : EngineObject
    {
        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is pressed by a <see cref="ScenePointerTracker"/>.
        /// </summary>
        public event UIElementPointerHandler Pressed;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is held by a <see cref="ScenePointerTracker"/>.
        /// </summary>
        public event UIElementDeltaPositionHandler Held;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is dragged by a <see cref="ScenePointerTracker"/>.
        /// </summary>
        public event UIElementDeltaPositionHandler Dragged;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is released by a <see cref="ScenePointerTracker"/>.
        /// </summary>
        public event UIElementPointerHandler Released;

        /// <summary>
        /// Invoked when a pointer or other form of input enters the bounds of the current <see cref="UIElement"/>.
        /// </summary>
        public event UIElementPositionHandler Enter;

        /// <summary>
        /// Invoked when a pointer or other form of input leaves the bounds of the current <see cref="UIElement"/>.
        /// </summary>
        public event UIElementPositionHandler Leave;

        /// <summary>
        /// Invoked when a pointer has hovered over the 
        /// </summary>
        public event UIElementPositionHandler Hovered;

        /// <summary>
        /// Invoked when the parent of the current <see cref="UIElement"/> has changed.
        /// </summary>
        public event UIParentChangedHandler ParentChanged;

        /// <summary>
        /// Invoked when the <see cref="UIManagerComponent"/> of the current <see cref="UIElement"/> has changed.
        /// </summary>
        public event UIManagerChangedHandler ManagerChanged;


        UIManagerComponent _manager;
        UIElement _parent;
        UITheme _theme;
        UIElementState _state;
        Rectangle _localBounds;
        Rectangle _globalBounds;
        Rectangle _renderBounds;

        public UIElement()
        {
            Children = new UIChildCollection(this, false);
            CompoundElements = new UIChildCollection(this, true);
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
            UpdateBounds(_parent?.RenderBounds);
        }

        protected void UpdateBounds()
        {
            UpdateBounds(_parent?._renderBounds);
        }

        private void UpdateBounds(Rectangle? parentBounds)
        {
            if (parentBounds != null)
            {
                _globalBounds = new Rectangle()
                {
                    X = parentBounds.Value.X + _localBounds.X,
                    Y = parentBounds.Value.Y + _localBounds.Y,
                    Width = _localBounds.Width,
                    Height = _localBounds.Height,
                };
            }
            else
            {
                _globalBounds = LocalBounds;
            }

            _renderBounds = _globalBounds;
            _renderBounds.Inflate(-Padding.Left, -Padding.Top, -Padding.Right, -Padding.Bottom);

            OnAdjustRenderBounds(ref _renderBounds);

            OnUpdateCompoundBounds();
            foreach (UIElement e in CompoundElements)
                e.UpdateBounds(_globalBounds);

            OnUpdateChildBounds();
            foreach (UIElement e in Children)
                e.UpdateBounds(_renderBounds);

            OnUpdateBounds();
        }

        /// <summary>
        /// Invoked when a theme has been applied to the current <see cref="UIElement"/>, or <see cref="State"/> has changed.
        /// </summary>
        protected virtual void ApplyTheme()
        {
            if (_theme == null)
                return;

            foreach (UIElement e in CompoundElements)
                e.Theme = _theme;

            foreach (UIElement e in Children)
                e.Theme = _theme;

            _theme?.ApplyStyle(this);
            UpdateBounds(Parent?.RenderBounds);
        }

        internal void Update(Timing time)
        {
            if (!IsEnabled)
                return;

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

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is picked by a pointer or other form of input.
        /// <para>Overriding this method allows custom picking detection to be implemented. For example, a polygonal-shaped UI element.</para>
        /// </summary>
        /// <param name="globalPos">The global picking position.</param>
        /// <returns></returns>
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
        /// <param name="globalPos">The global position of the pointer.</param>
        public virtual void OnEnter(Vector2F globalPos)
        {
            Vector2F localPos = globalPos - (Vector2F)_globalBounds.TopLeft;
            Enter?.Invoke(this, localPos, globalPos);
        }

        /// <summary>
        /// Invoked when a pointer leaves the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="globalPos">The global position of the pointer.</param>
        public virtual void OnLeave(Vector2F globalPos)
        {
            if (State == UIElementState.Hovered)
                State = UIElementState.Default;

            Vector2F localPos = globalPos - (Vector2F)_globalBounds.TopLeft;
            Leave?.Invoke(this, localPos, globalPos);
        }

        public virtual void OnPressed(ScenePointerTracker tracker)
        {
            if (State == UIElementState.Default || 
                State != UIElementState.Hovered || 
                State != UIElementState.Active)
            {
                State = UIElementState.Pressed;
                ParentWindow?.BringToFront();
                Pressed?.Invoke(this, tracker);
            }
        }

        public virtual void OnHeld(ScenePointerTracker tracker)
        {
            if (State == UIElementState.Pressed)
            {
                Vector2F localPos = tracker.Position - (Vector2F)_globalBounds.TopLeft;
                Held?.Invoke(this, tracker, localPos, tracker.Position, tracker.IntegerDelta);
            }
        }

        public virtual void OnDragged(ScenePointerTracker tracker)
        {
            if (State == UIElementState.Pressed)
            {
                Vector2F localPos = tracker.Position - (Vector2F)_globalBounds.TopLeft;
                Dragged?.Invoke(this, tracker, localPos, tracker.Position, tracker.IntegerDelta);
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

        /// <summary>
        /// Sends the current <see cref="UIElement"/> to the front of it's parents child stack, so it will be drawn on top of all other children.
        /// </summary>
        public void BringToFront()
        {
            if (Parent == null)
                return;

            if (IsCompoundChild)
                Parent.CompoundElements.BringToFront(this);
            else
                Parent.Children.BringToFront(this);
        }

        /// <summary>
        /// Sends the current <see cref="UIElement"/> to the back of it's parents child stack, so it will be drawn underneath all other children.
        /// </summary>
        public void SendToBack()
        {
            if (Parent == null)
                return;

            if (IsCompoundChild)
                Parent.CompoundElements.SendToBack(this);
            else
                Parent.Children.SendToBack(this);
        }

        protected override void OnDispose() { }

        /// <summary>
        /// Invoked when the bounds need to be updated on the current <see cref="UIElement"/>.
        /// </summary>
        protected virtual void OnUpdateBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of compound elements.
        /// </summary>
        protected virtual void OnUpdateCompoundBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of child elements.
        /// </summary>
        protected virtual void OnUpdateChildBounds() { }

        /// <summary>
        /// Invoked after the initial render-bounds calculation, giving the current <see cref="UIElement"/> a chance to make custom adjustments to it's render bounds.
        /// </summary>
        /// <param name="renderbounds">The render bounds <see cref="Rectangle"/>.</param>
        protected virtual void OnAdjustRenderBounds(ref Rectangle renderbounds) { }

        /// <summary>
        /// Invoked when the parent of the current <see cref="UIElement"/> has changed.
        /// </summary>
        /// <param name="oldParent">The old parent, or null if none.</param>
        /// <param name="newParent">The new parent, or null if none.</param>
        protected virtual void OnParentChanged(UIElement oldParent, UIElement newParent)
        {
            ParentChanged?.Invoke(oldParent, newParent);
        }

        /// <summary>
        /// Invoked when the <see cref="UIManagerComponent"/> of the current <see cref="UIElement"/> has changed.
        /// </summary>
        /// <param name="oldManager">The old manager, or null if none.</param>
        /// <param name="newManager">The new manager, or null if none.</param>
        protected virtual void OnManagerChanged(UIManagerComponent oldManager, UIManagerComponent newManager)
        {
            ManagerChanged?.Invoke(oldManager, newManager);
        }

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> should perform update its logic or internal state.
        /// </summary>
        /// <param name="time">An instance of <see cref="Timing"/>.</param>
        protected virtual void OnUpdate(Timing time)
        {
            // TODO update window open/close animation
        }

        internal void Render(SpriteBatcher sb)
        {
            if (!IsVisible)
                return;

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

            if(ChildRenderEnabled)
                Children.Render(sb, ref _renderBounds);
        }

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> should perform any custom rendering to display itself.
        /// </summary>
        /// <param name="sb"></param>
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
                UpdateBounds(Parent?.RenderBounds);
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
        /// Gets or sets whether clipping is enabled.
        /// </summary>
        public bool IsClipEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the current <see cref="UIElement"/> is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the current <see cref="UIElement"/> is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => State == UIElementState.Disabled;
            set
            {
                bool enabled = State != UIElementState.Disabled;

                if (value != enabled)
                {
                    foreach (UIElement e in CompoundElements)
                        e.IsEnabled = value;

                    // Changing the state triggers a recursive theme update, so we don't need to do it ourselves here.
                    State = value ? UIElementState.Default : UIElementState.Disabled;
                }
            }
        }

        /// <summary>
        /// Gets a read-only list of child components attached to the current <see cref="UIElement"/>.
        /// </summary>
        public UIChildCollection Children { get; }

        /// <summary>
        /// Gets a list of compound child <see cref="UIElement"/>. These can only be modified by the current <see cref="UIElement"/>.
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
                    UIElement oldParent = _parent;
                    _parent = value;
                    OnParentChanged(oldParent, _parent);
                    UpdateBounds(Parent?.RenderBounds);
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
                    UIManagerComponent oldManager = _manager;
                    _manager = value;
                    OnManagerChanged(oldManager, _manager);
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

        /// <summary>
        /// Gets the margin of the current <see cref="UIElement"/>. This spacing directly affects the <see cref="BorderBounds"/>.
        /// </summary>
        [DataMember]
        public UISpacing Margin { get; } = new UISpacing();

        /// <summary>
        /// Gets the padding of the current <see cref="UIElement"/>. This is the spacing between the <see cref="Margin"/> and <see cref="RenderBounds"/>.
        /// </summary>
        [DataMember]
        public UISpacing Padding { get; } = new UISpacing();

        [DataMember]
        public UIAnchorFlags Anchor { get; set; }

        /// <summary>
        /// Gets the <see cref="UIWindow"/> that contains the current <see cref="UIElement"/>.
        /// </summary>
        public UIWindow ParentWindow { get; internal set; }

        /// <summary>
        /// Gets or sets wher or not child <see cref="UIElement"/> objects
        /// </summary>
        protected bool ChildRenderEnabled { get; set; } = true;
    }
}
