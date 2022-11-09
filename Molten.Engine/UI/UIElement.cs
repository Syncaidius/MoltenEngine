using Molten.Graphics;
using Molten.Input;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Molten.UI
{
    public delegate void UIElementPointerHandler(UIElement element, UIPointerTracker tracker);

    public delegate void UIElementHandler(UIElement element);

    public delegate void UIElementHandler<T>(T element) where T : UIElement;

    public delegate void UIElementDeltaPositionHandler(UIElement element, UIPointerTracker tracker, Vector2F localPos, Vector2F globalPos, Vector2I delta);

    public delegate void UIElementScrollWheelHandler(UIElement element, InputScrollWheel wheel);

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
        /// Invoked when the current <see cref="UIElement"/> is pressed by a <see cref="UIPointerTracker"/>.
        /// </summary>
        public event UIElementPointerHandler Pressed;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is held by a <see cref="UIPointerTracker"/>.
        /// </summary>
        public event UIElementDeltaPositionHandler Held;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is dragged by a <see cref="UIPointerTracker"/>.
        /// </summary>
        public event UIElementDeltaPositionHandler Dragged;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is released by a <see cref="UIPointerTracker"/>.
        /// </summary>
        public event UIElementPointerHandler Released;

        /// <summary>
        /// Invoked when a pointer or other form of input enters the bounds of the current <see cref="UIElement"/>.
        /// </summary>
        public event UIElementPointerHandler Enter;

        /// <summary>
        /// Invoked when a pointer or other form of input leaves the bounds of the current <see cref="UIElement"/>.
        /// </summary>
        public event UIElementPointerHandler Leave;

        /// <summary>
        /// Invoked when a pointer has hovered over the 
        /// </summary>
        public event UIElementPointerHandler Hovered;

        /// <summary>
        /// Invoked when the parent of the current <see cref="UIElement"/> has changed.
        /// </summary>
        public event UIParentChangedHandler ParentChanged;

        /// <summary>
        /// Invoked when the <see cref="UIManagerComponent"/> of the current <see cref="UIElement"/> has changed.
        /// </summary>
        public event UIManagerChangedHandler ManagerChanged;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is focused.
        /// </summary>
        public event UIElementHandler Focused;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> is unfocused.
        /// </summary>
        public event UIElementHandler Unfocused;

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> with and/or height has changed.
        /// </summary>
        public event UIElementHandler Resized;

        public event UIElementScrollWheelHandler Scrolled;

        List<UIElementLayer> _layers;
        UIManagerComponent _manager;
        UIElementLayer _parentLayer;
        UITheme _theme;
        UIElementState _state;
        Vector2I _prevSize;
        Rectangle _localBounds;
        Rectangle _globalBounds;
        Rectangle _renderBounds;
        Rectangle _offsetRenderBounds;
        Vector2F _renderOffset;
        bool _isFocused;

        /// <summary>
        /// Creates a new instance of <see cref="UIElement"/>.
        /// </summary>
        public UIElement()
        {
            _layers = new List<UIElementLayer>();
            Engine = Engine.Current;

            BaseElements = AddLayer(UIElementLayerBoundsUsage.GlobalBounds);

            Type[] childFilter = OnGetChildFilter();
            Children = AddLayer(UIElementLayerBoundsUsage.RenderBounds, childFilter);

            State = UIElementState.Default;
            OnInitialize(Engine, Engine.Settings.UI);
            ApplyTheme();
        }

        /// <summary>
        /// Invoked in the <see cref="UIElement"/> constructor when requesting a filter for <see cref="Children"/>. The default return value is <see cref="null"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual Type[] OnGetChildFilter() { return null; }

        protected UIElementLayer AddLayer(UIElementLayerBoundsUsage boundsUsage, params Type[] filter)
        {
            UIElementLayer layer = new UIElementLayer(this, boundsUsage, filter);
            _layers.Add(layer);
            return layer;
        }

        protected UIElementLayer AddLayerBefore(UIElementLayer layer, UIElementLayerBoundsUsage boundsUsage, params Type[] filter)
        {
            int index = _layers.IndexOf(layer);
            UIElementLayer newLayer = new UIElementLayer(this, boundsUsage, filter);
            _layers.Insert(index, newLayer);
            return newLayer;
        }

        protected UIElementLayer AddLayerAfter(UIElementLayer layer, UIElementLayerBoundsUsage boundsUsage, params Type[] filter)
        {
            int index = _layers.IndexOf(layer);
            UIElementLayer newLayer = new UIElementLayer(this, boundsUsage, filter);
            _layers.Insert(index + 1, newLayer);

            return newLayer;
        }

        /// <summary>
        /// Removes a <see cref="UIElementLayer"/> from the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="layer"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void RemoveLayer(UIElementLayer layer)
        {
            if (layer == Children || layer == BaseElements)
                throw new InvalidOperationException("Cannot remove base or child element layers");

            _layers.Remove(layer);
        }

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> needs to be initialized.
        /// </summary>
        /// <param name="engine">The engine instance from which the initialization call is being performed.</param>
        /// <param name="settings">The UI settings instance that was provided to the <paramref name="engine"/> during its instantiation.</param>
        protected virtual void OnInitialize(Engine engine, UISettings settings)
        {
            Margin.OnChanged += Margin_OnChanged;
            Padding.OnChanged += Padding_OnChanged;
        }

        private void Margin_OnChanged(UIMargin margin, UIMargin.Side side)
        {
            UpdateBounds();
        }

        private void Padding_OnChanged(UIPadding value)
        {
            UpdateBounds();
        }

        /// <summary>
        /// Invoked to update bounds and positional information for the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="parentBounds">The bounds used to represent a parent <see cref="UIElement"/>, if any. 
        /// <para>If null, the actually <see cref="ParentElement"/> will be used, if any.</para></param>
        protected void UpdateBounds(Rectangle? parentBounds = null)
        {
            if (parentBounds == null && _parentLayer != null)
            {
                if (_parentLayer.BoundsUsage == UIElementLayerBoundsUsage.GlobalBounds)
                    parentBounds = _parentLayer.Owner._globalBounds;
                else
                    parentBounds = _parentLayer.Owner._offsetRenderBounds;
            }

            // Are parent bounds still null?
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

            _offsetRenderBounds = _renderBounds;
            _offsetRenderBounds.X += (int)_renderOffset.X;
            _offsetRenderBounds.Y += (int)_renderOffset.Y;

            OnPreUpdateLayerBounds();

            foreach (UIElementLayer layer in _layers)
            {
                if (!layer.IsEnabled)
                    continue;

                Rectangle pBounds = layer.BoundsUsage == UIElementLayerBoundsUsage.GlobalBounds ? 
                    _globalBounds :
                    _offsetRenderBounds;

                foreach (UIElement e in layer)
                    e.UpdateBounds(pBounds);
            }

            OnUpdateBounds();

            if (_localBounds.Width != _prevSize.X || _localBounds.Height != _prevSize.Y)
            {
                _prevSize.X = _localBounds.Width;
                _prevSize.Y = _localBounds.Height;
                Resized?.Invoke(this);
            }
        }

        /// <summary>
        /// Invoked when a theme has been applied to the current <see cref="UIElement"/>, or <see cref="State"/> has changed.
        /// </summary>
        protected virtual void ApplyTheme()
        {
            if (_theme == null)
                return;

            foreach(UIElementLayer layer in _layers)
            {
                foreach (UIElement e in layer)
                    e.Theme = _theme;
            }

            _theme?.ApplyStyle(this);
            UpdateBounds();
        }

        internal void Update(Timing time)
        {
            if (!IsEnabled)
                return;
                
            OnUpdate(time);

            foreach (UIElementLayer layer in _layers)
            {
                if (!layer.IsEnabled)
                    continue;

                for (int i = layer.Count - 1; i >= 0; i--)
                    layer[i].Update(time);
            }
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
        /// Switches focus to the current <see cref="UIElement"/>.
        /// </summary>
        public void Focus()
        {
            Manager.FocusedElement = this;
        }

        /// <summary>
        /// Unfocuses the current <see cref="UIElement"/> if focused.
        /// </summary>
        public void Unfocus()
        {
            if (Manager.FocusedElement == this)
                Manager.FocusedElement = null;
        }

        /// <summary>
        /// Returns the current <see cref="UIElement"/> or one of it's children (recursive), depending on which contains <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point to use for picking a <see cref="UIElement"/>.</param>
        /// <param name="ignoreRules">If true, <see cref="InputRules"/> is ignored when picking.</param>
        /// <returns></returns>
        public UIElement Pick(Vector2F point)
        {
            UIElement result = null;

            if (IsEnabled && Contains(point))
            {
                UIElementLayer layer;

                // Check each layer in reverse order.
                // The last layer is top-most, so we start there and work backwards to the first (lowest) layer.
                if (HasInputRules(UIInputRuleFlags.Children))
                {
                    for (int i = _layers.Count - 1; i >= 0; i--)
                    {
                        layer = _layers[i];

                        if (layer.IgnoreInput)
                            continue;

                        // Try picking one of the layer's elements.
                        for (int e = layer.Count - 1; e >= 0; e--)
                        {
                            result = layer[e].Pick(point);
                            if (result != null)
                                return result;
                        }
                    }
                }

                if(HasInputRules(UIInputRuleFlags.Self))
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
        /// <param name="tracker">The <see cref="UIPointerTracker"/> which triggered hovered over the current <see cref="UIElement"/>.</param>
        public virtual void OnHover(UIPointerTracker tracker)
        {
            if (State == UIElementState.Default)
                State = UIElementState.Hovered;

            Hovered?.Invoke(this, tracker);
        }

        /// <summary>
        /// Invoked when a pointer enters the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="tracker">The <see cref="UIPointerTracker"/> which triggered entered the current <see cref="UIElement"/> bounds.</param>
        public virtual void OnEnter(UIPointerTracker tracker)
        {
            Enter?.Invoke(this, tracker);
        }

        /// <summary>
        /// Invoked when a pointer leaves the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="tracker">The <see cref="UIPointerTracker"/> which triggered left the current <see cref="UIElement"/> bounds.</param>
        public virtual void OnLeave(UIPointerTracker tracker)
        {
            if (State == UIElementState.Hovered)
                State = UIElementState.Default;

            Leave?.Invoke(this, tracker);
        }

        public virtual void OnPressed(UIPointerTracker tracker)
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

        public virtual void OnHeld(UIPointerTracker tracker)
        {
            if (State == UIElementState.Pressed)
            {
                Vector2F localPos = tracker.Position - (Vector2F)_globalBounds.TopLeft;
                Held?.Invoke(this, tracker, localPos, tracker.Position, tracker.IntegerDelta);
            }
        }

        public virtual void OnDragged(UIPointerTracker tracker)
        {
            if (State == UIElementState.Pressed)
            {
                Vector2F localPos = tracker.Position - (Vector2F)_globalBounds.TopLeft;
                Dragged?.Invoke(this, tracker, localPos, tracker.Position, tracker.IntegerDelta);
            }
        }

        public virtual bool OnScrollWheel(InputScrollWheel wheel)
        {
            Scrolled?.Invoke(this, wheel);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="releasedOutside">The current <see cref="UIElement"/> was released outside of it's bounds.</param>
        public virtual void OnReleased(UIPointerTracker tracker, bool releasedOutside)
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
            if (ParentElement == null)
                return;

            ParentLayer?.BringToFront(this);
        }

        /// <summary>
        /// Sends the current <see cref="UIElement"/> to the back of it's parents child stack, so it will be drawn underneath all other children.
        /// </summary>
        public void SendToBack()
        {
            if (ParentElement == null)
                return;

            ParentLayer?.SendToBack(this);
        }

        protected override void OnDispose() { }

        /// <summary>
        /// Invoked when the bounds need to be updated on the current <see cref="UIElement"/>.
        /// </summary>
        protected virtual void OnUpdateBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of elements held in the underlying <see cref="UIElementLayer"/>s of the current <see cref="UIElement"/>.
        /// </summary>
        protected virtual void OnPreUpdateLayerBounds() { }

        /// <summary>
        /// Invoked when <see cref="LocalBounds"/> was changed. This allows adjustments or overrides to be applied to <see cref="LocalBounds"/> before <see cref="UpdateBounds()"/> is called internally.
        /// </summary>
        /// <param name="localBounds"></param>
        protected virtual void OnUpdateLocalBounds(ref Rectangle localBounds) { }

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
        protected virtual void OnUpdate(Timing time) { }

        protected virtual void OnFocused() { }

        protected virtual void OnUnfocused() { }

        internal void Render(SpriteBatcher sb)
        {
            if (!IsVisible)
                return;

            // Only render if we were given a valid clip. i.e. it is partially/fully inside the previous clip and therefore visible.
            if (sb.PushClip(_globalBounds))
            {
                OnRender(sb);

                for (int i = 0; i < _layers.Count; i++)
                    _layers[i].Render(sb);

                sb.PopClip();
            }
        }

        /// <summary>
        /// Invoked when the current <see cref="UIElement"/> should perform any custom rendering to display itself.
        /// </summary>
        /// <param name="sb"></param>
        protected virtual void OnRender(SpriteBatcher sb) { }

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
                OnUpdateLocalBounds(ref _localBounds);
                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets the X of <see cref="LocalBounds"/> for the current <see cref="UIElement"/>.
        /// </summary>
        public int X => _localBounds.X;

        /// <summary>
        /// Gets the Y of <see cref="LocalBounds"/> for the current <see cref="UIElement"/>.
        /// </summary>
        public int Y => _localBounds.Y;

        /// <summary>
        /// Gets the width of <see cref="LocalBounds"/> for the current <see cref="UIElement"/>.
        /// </summary>
        public int Width => _localBounds.Width;

        /// <summary>
        /// Gets the height of <see cref="LocalBounds"/> for the current <see cref="UIElement"/>.
        /// </summary>
        public int Height => _localBounds.Height;

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
        /// Gets the <see cref="RenderBounds"/> with <see cref="RenderOffset"/> applied to it.
        /// </summary>
        public Rectangle OffsetRenderBounds => _offsetRenderBounds;

        /// <summary>
        /// Gets or sets the offset of child elements rendered inside <see cref="RenderBounds"/>. This is useful for features such as scrolling.
        /// </summary>
        protected Vector2F RenderOffset
        {
            get => _renderOffset;
            set
            {
                _renderOffset = value;
                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="UIElement"/> is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the current <see cref="UIElement"/> is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => State != UIElementState.Disabled;
            set
            {
                bool enabled = State != UIElementState.Disabled;

                if (value != enabled)
                {
                    foreach (UIElementLayer layer in _layers)
                    {
                        foreach (UIElement e in layer)
                            e.IsEnabled = value;
                    }

                    // Changing the state triggers a recursive theme update, so we don't need to do it ourselves here.
                    State = value ? UIElementState.Default : UIElementState.Disabled;
                }
            }
        }

        /// <summary>
        /// Gets a read-only list of child components attached to the current <see cref="UIElement"/>.
        /// </summary>
        public UIElementLayer Children { get; }

        /// <summary>
        /// Gets a list of base child <see cref="UIElement"/> which help form the current <see cref="UIElement"/>. 
        /// <para>These can only be modified by the current <see cref="UIElement"/>.</para>
        /// </summary>
        protected UIElementLayer BaseElements { get; }

        /// <summary>
        /// Gets the parent <see cref="UIElementLayer"/> for the current <see cref="UIElement"/>, or null if no value is set. 
        /// </summary>
        public UIElementLayer ParentLayer
        {
            get => _parentLayer;
            internal set
            {
                if (_parentLayer != value)
                {
                    UIElementLayer oldParent = _parentLayer;
                    _parentLayer = value;
                    OnParentChanged(oldParent?.Owner, ParentElement);
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets the parent of the current <see cref="UIElement"/>.
        /// </summary>
        public UIElement ParentElement => _parentLayer?.Owner;

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

                    foreach (UIElementLayer layer in _layers)
                    {
                        foreach (UIElement e in layer)
                            e.Manager = value;
                    }
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
        /// Gets the margin of the current <see cref="UIElement"/>. This spacing directly affects the <see cref="BorderBounds"/>.
        /// </summary>
        [DataMember]
        public UIMargin Margin { get; } = new UIMargin();

        /// <summary>
        /// Gets the padding of the current <see cref="UIElement"/>. This is the spacing between the <see cref="Margin"/> and <see cref="RenderBounds"/>.
        /// </summary>
        [DataMember]
        public UIPadding Padding { get; } = new UIPadding();

        /// <summary>
        /// Gets the <see cref="UIWindow"/> that contains the current <see cref="UIElement"/>.
        /// </summary>
        public UIWindow ParentWindow { get; internal set; }

        /// <summary>
        /// Gets or sets whether or not the current <see cref="UIElement"/> is focused.
        /// </summary>
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if(_isFocused != value)
                {
                    _isFocused = value;

                    if (_isFocused)
                    {
                        OnFocused();
                        Focused?.Invoke(this);
                    }
                    else
                    {
                        OnUnfocused();
                        Unfocused?.Invoke(this);
                    }
                }
            }
        }
    }
}
