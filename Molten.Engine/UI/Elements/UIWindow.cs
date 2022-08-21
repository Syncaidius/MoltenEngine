using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UIWindow : UIElement
    {
        delegate void StateCallback(UIWindow window);

        class StateChange
        {
            public Rectangle TargetBounds;
            public StateCallback CustomCallback;

            public UIWindowState StartState;
            public UIWindowState EndState;

            public UIElementCancelHandler<UIWindow> StartEvent;
            public Func<bool> CheckMethod;

            public UIElementHandler<UIWindow> Event;
            public Action CompletionMethod;
            public UIElement NewParent;

            public StateChange(UIWindowState startState,
                UIElementCancelHandler<UIWindow> startEvent,
                Func<bool> startCheckMethod,
                UIWindowState endState,
                StateCallback callback)
            {
                StartState = startState;
                StartEvent = startEvent;
                Event = null;
                CheckMethod = startCheckMethod;
                EndState = endState;
                CompletionMethod = null;
                NewParent = null;
                CustomCallback = callback;
            }

            public StateChange(UIWindowState startState, UIElementHandler<UIWindow> sEvent, Action windowMethod, StateCallback callback)
            {
                StartState = startState;
                StartEvent = null;
                Event = sEvent;
                CheckMethod = null;
                EndState = startState;
                CompletionMethod = windowMethod;
                CustomCallback = callback;
            }
        }

        /// <summary>
        /// Invoked when <see cref="Minimize(bool)"/> was called on the current <see cref="UIWindow"/>. The opening can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Minimizing;

        /// <summary>
        /// Invoked when <see cref="Open(bool, UIElement)"/> was called on the current <see cref="UIWindow"/>. The opening can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Opening;

        /// <summary>
        /// Invoked when <see cref="Close(bool)"/> was called on the current <see cref="UIWindow"/>. The closure can be cancelled by any subscriber to this event.
        /// </summary>
        public event UIElementCancelHandler<UIWindow> Closing;

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished closing.
        /// </summary>
        public event UIElementHandler<UIWindow> Closed;

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished opening.
        /// </summary>
        public event UIElementHandler<UIWindow> Opened;

        /// <summary>
        /// Invoked when the current <see cref="UIWindow"/> has finished minimizing.
        /// </summary>
        public event UIElementHandler<UIWindow> Minimized;

        UIPanel _titleBar;
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UILabel _title;
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
                [UIWindowState.Opening] = new StateChange(UIWindowState.Opening, Opening, OnOpening, UIWindowState.Open, (window) =>
                {
                    ChildrenEnabled = false;
                    IsVisible = true;
                    Interpolate(_lerpBounds, _defaultBounds, false);
                }),

                [UIWindowState.Open] = new StateChange(UIWindowState.Open, Opened, OnOpened, (window) =>
                {
                    ChildrenEnabled = true;
                    IsVisible = true;
                    Interpolate(_lerpBounds, _defaultBounds, true);
                }),

                [UIWindowState.Closing] = new StateChange(UIWindowState.Closing, Closing, OnClosing, UIWindowState.Closed, (window) =>
                {
                    Interpolate(_lerpBounds, _closeBounds, false);
                    IsVisible = true;
                    ChildrenEnabled = false;
                }),

                [UIWindowState.Closed] = new StateChange(UIWindowState.Closed, Closed, OnClosed, (window) =>
                {
                    ChildrenEnabled = false;
                    IsVisible = false;
                    Interpolate(_lerpBounds, _closeBounds, true);
                }),

                [UIWindowState.Minimizing] = new StateChange(UIWindowState.Minimizing, Minimizing, OnMinimizing, UIWindowState.Minimized, (window) =>
                {
                    Interpolate(_lerpBounds, _minimizeBounds, false);
                    ChildrenEnabled = false;
                }),
                [UIWindowState.Minimized] = new StateChange(UIWindowState.Minimized, Minimized, OnMinimized, (window) =>
                {
                    ChildrenEnabled = false;
                    Interpolate(_lerpBounds, _minimizeBounds, true);
                }),
            };

            _titleBarButtons = new List<UIButton>();
            BorderThickness.OnChanged += BorderThickness_OnChanged;

            // Change _panel corners to only round bottom left/right.
            _titleBar = CompoundElements.Add<UIPanel>();
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UILabel>();
            _title.VerticalAlign = UIVerticalAlignment.Center;

            _btnClose = AddTitleButton("X");
            _btnClose.Pressed += _btnClose_Pressed;
            _btnMaximize = AddTitleButton("^");
            _btnMinimize = AddTitleButton("_");

            _titleBarHeight = 26;
            Title = Name;
        }

        private void BorderThickness_OnChanged()
        {
            ApplyBorderThickness();
            UpdateBounds();
        }

        private void ApplyBorderThickness()
        {
            _panel.BorderThickness.Apply(BorderThickness);
            _titleBar.BorderThickness.Apply(BorderThickness);
        }

        private void _btnClose_Pressed(UIElement element, ScenePointerTracker tracker)
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
            UIButton btn = CompoundElements.Add<UIButton>();
            _titleBarButtons.Add(btn);
            btn.Text = text;
            return btn;
        }

        protected override bool OnPicked(Vector2F globalPos)
        {
            return !RenderBounds.Contains(globalPos);
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            Rectangle gb = GlobalBounds;

            _titleBar.LocalBounds = new Rectangle(0, 0, gb.Width, TitleBarHeight);
            _panel.LocalBounds = new Rectangle(0, TitleBarHeight, gb.Width, gb.Height - TitleBarHeight);
            _title.LocalBounds = new Rectangle(TitleBarHeight + (_iconSpacing * 2), 0, gb.Width, TitleBarHeight);

            for (int i = 0; i < _titleBarButtons.Count; i++)
            {
                int pX = gb.Width - (TitleBarHeight * (i + 1));
                _titleBarButtons[i].LocalBounds = new Rectangle(pX, 0, TitleBarHeight, TitleBarHeight);
            }

            _minimizeBounds.Width = (_titleBarButtons.Count * TitleBarHeight) + _iconSpacing + (int)_title.MeasuredSize.X;
            _minimizeBounds.Height = _titleBarHeight;
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            renderbounds.Inflate(-BorderThickness.Left, -(BorderThickness.Top + _titleBarHeight), -BorderThickness.Right, -BorderThickness.Bottom);
        }

        public override void OnPressed(ScenePointerTracker tracker)
        {
            base.OnPressed(tracker);
            BringToFront();
        }

        public override void OnDragged(ScenePointerTracker tracker)
        {
            base.OnDragged(tracker);
            LocalBounds += tracker.IntegerDelta;
        }

        protected virtual bool OnClosing() { return true; }

        protected virtual void OnClosed() { }

        protected virtual bool OnOpening() { return true; }

        protected virtual void OnOpened() { }

        protected virtual bool OnMinimizing() { return true; }

        protected virtual void OnMinimized() { }

        protected override void OnUpdateLocalBounds(ref Rectangle localBounds)
        {
            if (WindowState != UIWindowState.Open)
            {
                localBounds = _lerpBounds;
            }
            else
            {
                _defaultBounds = localBounds;
                _lerpBounds = localBounds;

                _maximizeBounds = Parent != null ? Parent.RenderBounds : _defaultBounds;
                _minimizeBounds.X = _defaultBounds.X;
                _minimizeBounds.Y = _defaultBounds.Y;
                _closeBounds = new Rectangle(_defaultBounds.Center.X, _defaultBounds.Center.Y, 10, 10);
            }
        }

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

            UICancelEventArgs args = InvokeCancelableHandler(_curChange.StartEvent, this);
            if (_curChange.CheckMethod?.Invoke() == false || args.Cancel)
                return;

            _curChange.CustomCallback?.Invoke(this);
            WindowState = _curChange.StartState;

            _curChange.Event?.Invoke(this);

            if (_curChange.NewParent != null)
                Parent = _curChange.NewParent;

            // Call the completion method. This is likely to be a UIWindow method. e.g. OnOpened, OnClosed or OnMinimized.
            _curChange.CompletionMethod?.Invoke();
        }

        public void Open(bool immediate = false, UIElement newParent = null)
        {
            UIWindowState state = immediate ? UIWindowState.Open : UIWindowState.Opening;
            _changes[state].NewParent = newParent;

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

        public void Minimize(bool immediate = false)
        {
            StartState(immediate ? UIWindowState.Minimized : UIWindowState.Minimizing);
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
        public UISpacing BorderThickness { get; } = new UISpacing(2);

        /// <summary>
        /// Gets the <see cref="UIWindowState"/> of the current <see cref="UIWindow"/>.
        /// </summary>
        public UIWindowState WindowState { get; private set; }

        /// <summary>
        /// Gets or sets the rate at which the current <see cref="UIWindow"/> open/close animation will run. The minimum speed is 0.05f;
        /// </summary>
        public float AnimationSpeed
        {
            get => _expandRate;
            set => Math.Max(0.05f, _expandRate);
        }
    }
}
