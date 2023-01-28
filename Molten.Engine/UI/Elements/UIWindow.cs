using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UIWindow : UIElement, IWindow
    {
        delegate void StateCallback(UIWindow window);

        class StateChange
        {
            public event UIElementCancelHandler<UIWindow> Starting;

            public event UIElementHandler<UIWindow> Completed;

            public Rectangle TargetBounds;
            public StateCallback CustomCallback;

            public UIWindowState StartState;
            public UIWindowState EndState;

            public Func<bool> CheckMethod;

            public Action CompletionMethod;
            public UIElementLayer NewParentLayer;

            public StateChange(UIWindowState startState,
                Func<bool> startCheckMethod,
                UIWindowState endState,
                StateCallback callback)
            {
                StartState = startState;
                CheckMethod = startCheckMethod;
                EndState = endState;
                CompletionMethod = null;
                NewParentLayer = null;
                CustomCallback = callback;
            }

            public StateChange(UIWindowState startState, Action windowMethod, StateCallback callback)
            {
                StartState = startState;
                CheckMethod = null;
                EndState = startState;
                CompletionMethod = windowMethod;
                CustomCallback = callback;
            }

            /// <summary>
            /// Invokes a <see cref="UIElementCancelHandler{T}"/> event, one subscriber at a time, using a <see cref="UICancelEventArgs"/> instance to check for a cancellation.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="handler">The <see cref="UIElementCancelHandler{T}"/> event to be invoked.</param>
            /// <param name="element">The element to invoke the event upon.</param>
            /// <returns></returns>
            public UICancelEventArgs InvokeStartingEvent<T>(T element)
                where T : UIElement
            {
                UICancelEventArgs args = new UICancelEventArgs();

                // Iterate over Starting event subscribers to check if any of them want to block the state change.
                if (Starting != null)
                {
                    Delegate[] handlers = Starting.GetInvocationList();

                    foreach (UIElementCancelHandler<T> sub in handlers)
                    {
                        sub(element, args);
                        if (args.Cancel)
                            break;
                    }
                }

                return args;
            }

            public void InvokeCompletion(UIWindow element)
            {
                Completed?.Invoke(element);
            }
        }

        /// <summary>
        /// Invoked when <see cref="Maximize(bool)"/> was called on the current <see cref="UIWindow"/>. The opening can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Maximizing
        {
            add => _changes[UIWindowState.Maximizing].Starting += value;
            remove => _changes[UIWindowState.Maximizing].Starting -= value;
        }

        /// <summary>
        /// Invoked when <see cref="Minimize(bool)"/> was called on the current <see cref="UIWindow"/>. The opening can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Minimizing
        {
            add => _changes[UIWindowState.Minimizing].Starting += value;
            remove => _changes[UIWindowState.Minimizing].Starting -= value;
        }

        /// <summary>
        /// Invoked when <see cref="Open(bool, UIElement)"/> was called on the current <see cref="UIWindow"/>. The opening can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Opening
        {
            add => _changes[UIWindowState.Opening].Starting += value;
            remove => _changes[UIWindowState.Opening].Starting -= value;
        }

        /// <summary>
        /// Invoked when <see cref="Close(bool)"/> was called on the current <see cref="UIWindow"/>. The closure can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Closing
        {
            add => _changes[UIWindowState.Closing].Starting += value;
            remove => _changes[UIWindowState.Closing].Starting -= value;
        }

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished closing.
        /// </summary>
        public event UIElementHandler<UIWindow> Closed
        {
            add => _changes[UIWindowState.Closed].Completed += value;
            remove => _changes[UIWindowState.Closed].Completed -= value;
        }

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished opening.
        /// </summary>
        public event UIElementHandler<UIWindow> Opened
        {
            add => _changes[UIWindowState.Open].Completed += value;
            remove => _changes[UIWindowState.Open].Completed -= value;
        }

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished minimizing.
        /// </summary>
        public event UIElementHandler<UIWindow> Minimized
        {
            add => _changes[UIWindowState.Minimized].Completed += value;
            remove => _changes[UIWindowState.Minimized].Completed -= value;
        }

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished maximizing.
        /// </summary>
        public event UIElementHandler<UIWindow> Maximized
        {
            add => _changes[UIWindowState.Maximized].Completed += value;
            remove => _changes[UIWindowState.Maximized].Completed -= value;
        }

        UIPanel _titleBar;
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UILabel _title;
        UISprite _icon;
        int _titleBarHeight;
        int _iconSpacing = 5;
        List<UIButton> _titleBarButtons;

        Color _borderColor = new Color(52, 189, 235, 255);
        Color _fillColor = new Color(0, 109, 155, 200);
        CornerInfo _cornerRadius = new CornerInfo(8f);

        Rectangle _defaultBounds;
        Rectangle _minimizeBounds;
        Rectangle _maximizeBounds;
        Rectangle _closeBounds;

        Dictionary<UIWindowState, StateChange> _changes;
        StateChange _curChange;

        Rectangle _lerpStartBounds;
        Rectangle _lerpEndBounds;
        Rectangle _lerpBounds;
        float _lerpPercent = 1f;
        float _expandRate = 0.2f;
        float _lerpMultiplier = 1f;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _changes = new Dictionary<UIWindowState, StateChange>()
            { 
                [UIWindowState.Opening] = new StateChange(UIWindowState.Opening, OnOpening, UIWindowState.Open, (window) =>
                {
                    Children.IsEnabled = false;
                    IsVisible = true;
                    _btnMinimize.IsVisible = true;
                    Interpolate(_lerpBounds, _defaultBounds, false);
                }),

                [UIWindowState.Open] = new StateChange(UIWindowState.Open, OnOpened, (window) =>
                {
                    Children.IsEnabled = true;
                    IsVisible = true;
                    _btnMinimize.IsVisible = true;
                    Interpolate(_lerpBounds, _defaultBounds, true);
                    // TODO set maximize icon to 'maximize'.
                }),

                [UIWindowState.Closing] = new StateChange(UIWindowState.Closing, OnClosing, UIWindowState.Closed, (window) =>
                {
                    Interpolate(_lerpBounds, _closeBounds, false);
                    IsVisible = true;
                    Children.IsEnabled = false;
                }),

                [UIWindowState.Closed] = new StateChange(UIWindowState.Closed, OnClosed, (window) =>
                {
                    Children.IsEnabled = false;
                    IsVisible = false;
                    Interpolate(_lerpBounds, _closeBounds, true);
                }),

                [UIWindowState.Minimizing] = new StateChange(UIWindowState.Minimizing, OnMinimizing, UIWindowState.Minimized, (window) =>
                {
                    Interpolate(_lerpBounds, _minimizeBounds, false);
                    Children.IsEnabled = false;
                }),

                [UIWindowState.Minimized] = new StateChange(UIWindowState.Minimized, OnMinimized, (window) =>
                {
                    Children.IsEnabled = false;
                    _btnMinimize.IsVisible = false;
                    Interpolate(_lerpBounds, _minimizeBounds, true);
                    // Set maximize icon to 'restore'
                }),

                [UIWindowState.Maximizing] = new StateChange(UIWindowState.Maximizing, OnMaximizing, UIWindowState.Maximized, (window) =>
                {
                    Interpolate(_lerpBounds, _maximizeBounds, false);
                    _btnMinimize.IsVisible = true;
                    Children.IsEnabled = false;
                }),

                [UIWindowState.Maximized] = new StateChange(UIWindowState.Maximized, OnMaximized, (window) =>
                {
                    Children.IsEnabled = true;
                    _btnMinimize.IsVisible = true;
                    Interpolate(_lerpBounds, _maximizeBounds, true);
                    // TODO set maximize button icon to 'restore'.
                }),
            };

            _titleBarButtons = new List<UIButton>();
            BorderThickness.OnChanged += BorderThickness_OnChanged;

            // Change _panel corners to only round bottom left/right.
            _titleBar = BaseElements.Add<UIPanel>();
            _panel = BaseElements.Add<UIPanel>();
            _title = BaseElements.Add<UILabel>();
            _title.VerticalAlign = UIVerticalAlignment.Center;
            _icon = BaseElements.Add<UISprite>();

            _btnClose = AddTitleButton("X");
            _btnClose.Pressed += _btnClose_Pressed;

            _btnMaximize = AddTitleButton("^");
            _btnMaximize.Pressed += _btnMaximize_Pressed;

            _btnMinimize = AddTitleButton("_");
            _btnMinimize.Pressed += _btnMinimize_Pressed;

            _titleBarHeight = 26;
            Title = Name;
        }

        private void _btnMaximize_Pressed(UIElement element, CameraInputTracker tracker)
        {
            if (WindowState == UIWindowState.Open)
                StartState(UIWindowState.Maximizing);
            else
                StartState(UIWindowState.Opening);
        }

        private void _btnMinimize_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Minimize();
        }

        private void BorderThickness_OnChanged(UIPadding value)
        {
            ApplyBorderThickness();
            UpdateBounds();
        }

        private void ApplyBorderThickness()
        {
            _panel.BorderThickness.Apply(BorderThickness);
            _titleBar.BorderThickness.Apply(BorderThickness);
        }

        private void _btnClose_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Close(false);
        }

        protected override void ApplyTheme()
        {
            base.ApplyTheme();

            _titleBar.CornerRadius.Set(_cornerRadius.TopLeft, _cornerRadius.TopRight, 0, 0);
            _titleBar.FillColor = _borderColor;
            _panel.CornerRadius.Set(0, 0, _cornerRadius.BottomRight, _cornerRadius.BottomLeft);
            ApplyBorderThickness();
        }

        private UIButton AddTitleButton(string text)
        {
            UIButton btn = BaseElements.Add<UIButton>();
            _titleBarButtons.Add(btn);
            btn.Text = text;
            return btn;
        }

        protected override bool OnPicked(Vector2F globalPos)
        {
            return true;
        }

        protected override void OnPreUpdateLayerBounds()
        {
            base.OnPreUpdateLayerBounds();

            Rectangle gb = GlobalBounds;
            int iconSize = TitleBarHeight - _iconSpacing;
            int iconWithSpacing = iconSize + (_iconSpacing * 2);

            _titleBar.LocalBounds = new Rectangle(0, 0, gb.Width, TitleBarHeight);
            _panel.LocalBounds = new Rectangle(0, TitleBarHeight, gb.Width, gb.Height - TitleBarHeight);
            _icon.LocalBounds = new Rectangle(_iconSpacing, _iconSpacing, iconSize, iconSize);
            _title.LocalBounds = new Rectangle(iconWithSpacing, 0, gb.Width, TitleBarHeight);

            int bX = gb.Width;
            foreach (UIButton button in _titleBarButtons)
            {
                if (!button.IsVisible)
                    continue;

                bX -= TitleBarHeight;
                button.LocalBounds = new Rectangle(bX, 0, TitleBarHeight, TitleBarHeight);
            }

            _minimizeBounds.Width = (gb.Width - bX) + iconWithSpacing + (int)_title.MeasuredSize.X;
            _minimizeBounds.Height = _titleBarHeight;
        }

        protected override void OnUpdateLocalBounds(ref Rectangle localBounds)
        {
            if (WindowState != UIWindowState.Open)
            {
                _lerpBounds.X = localBounds.X;
                _lerpBounds.Y = localBounds.Y;

                localBounds = _lerpBounds;
            }
            else
            {
                _defaultBounds = localBounds;
                _lerpBounds = localBounds;
            }
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds(); 
            
            _maximizeBounds = ParentElement != null ? ParentElement.OffsetRenderBounds : _defaultBounds;
            _minimizeBounds.X = _defaultBounds.X;
            _minimizeBounds.Y = _defaultBounds.Y;
            _closeBounds = new Rectangle(_defaultBounds.Center.X, _defaultBounds.Center.Y, 10, 10);
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            renderbounds.Inflate(-BorderThickness.Left, -(BorderThickness.Top + _titleBarHeight), -BorderThickness.Right, -BorderThickness.Bottom);
        }

        public override void OnPressed(CameraInputTracker tracker)
        {
            base.OnPressed(tracker);
            BringToFront();
        }

        public override void OnDragged(CameraInputTracker tracker)
        {
            base.OnDragged(tracker);

            if (WindowState != UIWindowState.Minimized && WindowState != UIWindowState.Open)
                return;

            LocalBounds += tracker.IntegerDelta;
        }

        protected virtual bool OnClosing() { return true; }

        protected virtual void OnClosed() { }

        protected virtual bool OnOpening() { return true; }

        protected virtual void OnOpened() { }

        protected virtual bool OnMinimizing() { return true; }

        protected virtual void OnMinimized() { }

        protected virtual bool OnMaximizing() { return true; }

        protected virtual void OnMaximized() { }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (_curChange != null && _lerpPercent < 1f)
            {
                _lerpBounds = Rectangle.Lerp(_lerpStartBounds, _lerpEndBounds, _lerpPercent);
                LocalBounds = _lerpBounds;

                _lerpPercent = Math.Min(1f, _lerpPercent + (_lerpMultiplier * _expandRate));
                if (_lerpPercent == 1f)
                {
                    if (_curChange.StartState != _curChange.EndState)
                        StartState(_curChange.EndState);
                }
            }
        }

        private void Interpolate(Rectangle start, Rectangle end, bool immediate)
        {
            if (!immediate)
            {
                _lerpPercent = 0f;
                _lerpMultiplier = 1f; // TODO alter this to be based on the largest dimension of end / start, compared to the _closeBounds / _defaultBounds rate (1f)
                _lerpStartBounds = start;
                _lerpBounds = start;
                _lerpEndBounds = end;
            }
            else
            {
                _lerpPercent = 1f;
                _lerpBounds = end;
                LocalBounds = _lerpBounds;
            }
        }

        private void StartState(UIWindowState state)
        {
            if (!_changes.TryGetValue(state, out _curChange))
                throw new NullReferenceException($"A StateChange instance for UIWindowState.{state} has not been created. Fix this!");

            if (WindowState == _curChange.StartState || WindowState == _curChange.EndState)
                return;

            UICancelEventArgs args = _curChange.InvokeStartingEvent(this);
            if (_curChange.CheckMethod?.Invoke() == false || args.Cancel)
                return;

            _curChange.CustomCallback?.Invoke(this);
            WindowState = _curChange.StartState;

            _curChange.InvokeCompletion(this);

            if (_curChange.NewParentLayer != null)
                ParentLayer = _curChange.NewParentLayer;

            // Call the completion method. This is likely to be a UIWindow method. e.g. OnOpened, OnClosed or OnMinimized.
            _curChange.CompletionMethod?.Invoke();
        }

        public void Open(bool immediate = false, UIElementLayer newParentLayer = null)
        {
            UIWindowState state = immediate ? UIWindowState.Open : UIWindowState.Opening;
            _changes[state].NewParentLayer = newParentLayer;

            StartState(state);
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="immediate">If true, the window will skip its closing animation and immediately close.</param>
        public void Close(bool immediate = false)
        {
            StartState(immediate ? UIWindowState.Closed : UIWindowState.Closing);
        }

        void IWindow.Close()
        {
            Close(false);
        }

        public void Minimize(bool immediate = false)
        {
            if (WindowState == UIWindowState.Closing || WindowState == UIWindowState.Closed)
                return;

            StartState(immediate ? UIWindowState.Minimized : UIWindowState.Minimizing);
        }

        public void Maximize(bool immediate = false)
        {
            if (WindowState == UIWindowState.Closing || WindowState == UIWindowState.Closed)
                return;

            StartState(immediate ? UIWindowState.Maximized : UIWindowState.Maximizing);
        }

        /// <summary>
        /// The title of the current <see cref="UIWindow"/>.
        /// </summary>
        [JsonProperty]
        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        /// <summary>
        /// The title bar height of the current <see cref="UIWindow"/>, in pixels. This value will also be the size of any buttons on the title bar.
        /// </summary>
        [JsonProperty]
        public int TitleBarHeight
        {
            get => _titleBarHeight;
            set
            {
                if (_titleBarHeight != value)
                {
                    _titleBarHeight = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border <see cref="Color"/> of the current <see cref="UIWindow"/>.
        /// </summary>
        [UIThemeMember]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _titleBar.FillColor = _borderColor;
                _titleBar.BorderColor = _borderColor;
                _panel.BorderColor = _borderColor;
            }
        }

        /// <summary>
        /// Gets or sets the fill/background <see cref="Color"/> of the current <see cref="UIWindow"/>.
        /// </summary>
        [UIThemeMember]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                _panel.FillColor = _fillColor;
            }
        }

        /// <summary>
        /// Gets or sets the corner radius of the current <see cref="UIWindow"/>.
        /// </summary>
        [UIThemeMember]
        public CornerInfo CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;

                _titleBar.CornerRadius.Set(_cornerRadius.TopLeft, _cornerRadius.TopRight, 0, 0);
                _panel.CornerRadius.Set(0, 0, _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                if (_titleBarButtons.Count > 0)
                {
                    CornerInfo btnRadius = _titleBarButtons[0].CornerRadius;
                    btnRadius.TopRight = _cornerRadius.TopRight;
                    _titleBarButtons[0].CornerRadius = btnRadius;
                }
            }
        }

        /// <summary>
        /// Gets or sets the border thickness of the current <see cref="UIWindow"/>.
        /// </summary>
        [UIThemeMember]
        public UIPadding BorderThickness { get; } = new UIPadding(2);

        /// <summary>
        /// Gets the <see cref="UIWindowState"/> of the current <see cref="UIWindow"/>.
        /// </summary>
        public UIWindowState WindowState { get; private set; }

        /// <summary>
        /// Gets or sets the rate at which the current <see cref="UIWindow"/> open/close animation will run. The minimum speed is 0.05f;
        /// </summary>
        [UIThemeMember]
        public float AnimationSpeed
        {
            get => _expandRate;
            set => Math.Max(0.05f, _expandRate);
        }

        /// <summary>
        /// Gets or sets the icon sprite of the current <see cref="UIWindow"/>.
        /// </summary>
        public Sprite Icon
        {
            get => _icon.Sprite;
            set => _icon.Sprite = value;
        }
    }
}
