using Molten.IO;
using Molten.Graphics;
using Molten.IO;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIWindow : UICompoundComponent
    {
        const int BORDER_SIZE = 3;
        internal const int ICON_SIZE = 16;
        internal const int ICON_SPACING = 4;

        /// <summary>Represents the various possible states of a UI window.</summary>
        public enum UIWindowState
        {
            Open = 1,
            
            Closed = 2,

            Minimized = 3,
        }

        Color _colorBorder;
        Color _colorBackground;
        Color _colorBorderFocused;

        Rectangle _titleBounds;
        Rectangle _originalBounds;

        UIRenderedText _text;

        UIButton _closeButton;
        UIButton _minimizeButton;

        bool _canMinimize = true;
        bool _canClose = true;

        bool _titleBarGrabbed;
        UIWindowState _state = UIWindowState.Open;

        ITexture2D _icon;

        /// <summary>Triggered when the window is opened.</summary>
        public event UIComponentHandler<UIWindow> OnOpen;

        /// <summary>Triggered when the window is closed.</summary>
        public event UIComponentHandler<UIWindow> OnClose;

        /// <summary>Triggered when the window is minimized.</summary>
        public event UIComponentHandler<UIWindow> OnMinimize;

        /// <summary>Triggered when the window is restored from a minimized state.</summary>
        public event UIComponentHandler<UIWindow> OnRestore;

        public UIWindow(Engine engine) : base(engine)
        {
            _text = new UIRenderedText(engine);
            _text.VerticalAlignment = UIVerticalAlignment.Center;
            _text.Text = " ";

            _colorBorder = new Color(150, 150, 150, 255);
            _colorBorderFocused = new Color(0, 122, 204, 255);
            _colorBackground = new Color(20, 40, 60, 255);

            _closeButton = new UIButton(engine);
            _closeButton.Text.Text = "X";
            _closeButton.DefaultColor = new Color(150, 150, 200, 255);
            _closeButton.ClickColor = new Color(200, 200, 255, 255);
            _closeButton.HoverColor = new Color(180, 180, 210, 255);
            _closeButton.OnClickEnded += _closeButton_OnClickEnded;

            _minimizeButton = new UIButton(engine);
            _minimizeButton.Text.Text = "_";
            _minimizeButton.DefaultColor = new Color(150, 150, 200, 255);
            _minimizeButton.ClickColor = new Color(200, 200, 255, 255);
            _minimizeButton.HoverColor = new Color(180, 180, 210, 255);
            _minimizeButton.OnClickEnded += _minimizeButton_OnClickEnded;

            AddPart(_closeButton);
            AddPart(_minimizeButton);

            OnClickStarted += UIWindow_OnPressStarted;
            OnDrag += UIWindow_OnDrag;
            OnClickEnded += UIWindow_OnPressCompleted;
            OnClickEndedOutside += UIWindow_OnPressCompletedOutside;

            _clipPadding.Left = BORDER_SIZE;
            _clipPadding.Right = BORDER_SIZE;
            _clipPadding.Bottom = BORDER_SIZE;

            _ui.WindowManager.RegisterWindow(this);
        }

        void _minimizeButton_OnClickEnded(UIEventData<MouseButton> data)
        {
            Minimize();
        }

        void _closeButton_OnClickEnded(UIEventData<MouseButton> data)
        {
            Close();
        }

        void UIWindow_OnPressCompletedOutside(UIEventData<MouseButton> data)
        {
            _titleBarGrabbed = false;
        }

        void UIWindow_OnPressCompleted(UIEventData<MouseButton> data)
        {
            _titleBarGrabbed = false;

            if (_state == UIWindowState.Minimized)
                _ui.WindowManager.Restore(this);
        }

        void UIWindow_OnDrag(UIEventData<MouseButton> data)
        {
            if (_titleBarGrabbed && _state == UIWindowState.Open)
            {
                Rectangle local = _localBounds;
                local.X += (int)data.Delta.X;
                local.Y += (int)data.Delta.Y;

                Rectangle parentBounds = _parent.GlobalBounds;
                if (local.X < 0) local.X = 0;
                if (local.Y < 0) local.Y = 0;

                if (local.Right > _parent.Width) local.X = _parent.Width - local.Width;
                if (local.Bottom > _parent.Height) local.Y = _parent.Height - local.Height;

                LocalBounds = local;
            }
        }

        void UIWindow_OnPressStarted(UIEventData<MouseButton> data)
        {
            Focus();
            BringToFront();

            if (data.InputValue != MouseButton.Left)
                return;

            _titleBarGrabbed = _titleBounds.Contains(data.Position);
        }

        protected override void OnApplyClipPadding()
        {
            switch (_state)
            {
                case UIWindowState.Open:
                    Vector2 textSize = _text.GetSize();
                    int titleHeight = (int)textSize.Y + 4;

                    _clipPadding.Top = titleHeight;
                    break;

                case UIWindowState.Minimized:
                    _clipPadding.Top = BORDER_SIZE;
                    break;
            }
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            bool hasMinimize = _canMinimize;
            bool hasClose = _canClose;

            switch (_state)
            {
                case UIWindowState.Open:
                    Vector2 textSize = _text.GetSize();
                    int titleHeight = (int)textSize.Y + 4;

                    _titleBounds = new Rectangle()
                    {
                        X = _globalBounds.X,
                        Y = _globalBounds.Y,
                        Width = _globalBounds.Width - (titleHeight * 2),
                        Height = titleHeight,
                    };

                    _closeButton.LocalBounds = new Rectangle()
                    {
                        X = _globalBounds.Right - titleHeight,
                        Y = _globalBounds.Y,
                        Width = titleHeight,
                        Height = titleHeight,
                    };

                    if (_canClose == false)
                    {
                        _minimizeButton.LocalBounds = _closeButton.LocalBounds;
                    }
                    else
                    {
                        _minimizeButton.LocalBounds = new Rectangle()
                        {
                            X = _closeButton.LocalBounds.X - titleHeight,
                            Y = _globalBounds.Y,
                            Width = titleHeight,
                            Height = titleHeight,
                        };
                    }

                    // Set text bounds
                    _text.Bounds = new Rectangle()
                    {
                        X = _titleBounds.X + ICON_SIZE + 4,
                        Y = _titleBounds.Y,
                        Width = _titleBounds.Width - ICON_SIZE - 4,
                        Height = _titleBounds.Height,
                    };

                    break;

                case UIWindowState.Minimized:
                    int iconArea = ICON_SIZE + ICON_SPACING;

                    _text.Bounds = new Rectangle()
                    {
                        X = _clippingBounds.X + iconArea,
                        Y = _clippingBounds.Y,
                        Width = _clippingBounds.Width - iconArea,
                        Height = _clippingBounds.Height,
                    };

                    hasClose = false;
                    hasMinimize = false;
                    break;
            }

            // Configure button visibility.
            _closeButton.IsEnabled = hasClose;
            _closeButton.IsVisible = hasClose;

            _minimizeButton.IsEnabled = hasMinimize;
            _minimizeButton.IsVisible = hasMinimize;
        }

        protected override void OnRender(ISpriteBatch sb)
        {
            base.OnRender(sb);

            switch (_state)
            {
                case UIWindowState.Open:
                    if (_ui.Focused == this)
                        sb.Draw(_globalBounds, _colorBorderFocused);
                    else
                        sb.Draw(_globalBounds, _colorBorder);

                    sb.Draw(_clippingBounds, _colorBackground);

                    _text.Draw(sb);

                    break;

                case UIWindowState.Minimized:
                    sb.Draw(_globalBounds, _colorBorderFocused);
                    sb.Draw(_clippingBounds, _colorBackground);
                    _text.Draw(sb);
                    break;
            }
        }

        protected override void OnRenderChildren(ISpriteBatch sb)
        {
            if (_state == UIWindowState.Open)
            {
                _children.ForInterlock(0, 1, (id, child) =>
                {
                    if (child.IsVisible == true)
                        child.Render(sb);

                    return false;
                });
            }
        }

        protected override void OnDispose()
        {
            _ui.WindowManager.UnregisterWindow(this);

            base.OnDispose();
        }

        public void Open()
        {
            _visible = true;
            _enabled = true;
            _state = UIWindowState.Open;
            if (_originalBounds == Rectangle.Empty)
                _originalBounds = LocalBounds;
            else
                LocalBounds = _originalBounds;

            OnOpen?.Invoke(this);
            _ui.WindowManager.Open(this);
        }

        public void Close()
        {
            _visible = false;
            _enabled = false;
            _state = UIWindowState.Closed;

            OnClose?.Invoke(this);
            _ui.WindowManager.Close(this);
        }

        public void Minimize()
        {
            _originalBounds = _localBounds;
            _state = UIWindowState.Minimized;

            if (OnMinimize != null)
                OnMinimize(this);

            _ui.WindowManager.Minimize(this);
        }

        public void Restore()
        {
            _visible = true;
            _enabled = true;
            _state = UIWindowState.Open;
            LocalBounds = _originalBounds;

            if (OnRestore != null)
                OnRestore(this);

            _ui.WindowManager.Restore(this);
        }

        [DataMember]
        [ExpandablePropertyAttribute]
        public UIRenderedText Title
        {
            get { return _text; }
        }

        [Browsable(false)]
        /// <summary>Gets the state of the window.</summary>
        public UIWindowState State
        {
            get { return _state; }
        }
        
        [DataMember]
        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                _canClose = value;
                OnUpdateBounds();
            }
        }

        [DataMember]
        public bool CanMinimize
        {
            get { return _canMinimize; }
            set
            {
                _canMinimize = value;
                OnUpdateBounds();
            }
        }

        [Category("Appearance")]
        [DisplayName("Border Color")]
        [DataMember]
        public Color BorderColor
        {
            get { return _colorBorder; }
            set { _colorBorder = value; }
        }

        [Category("Appearance")]
        [DisplayName("Focused Border Color")]
        [DataMember]
        public Color BorderColorFocused
        {
            get { return _colorBorderFocused; }
            set { _colorBorderFocused = value; }
        }

        [Category("Appearance")]
        [DisplayName("Background Color")]
        [DataMember]
        public Color BackgroundColor
        {
            get { return _colorBackground; }
            set { _colorBackground = value; }
        }
    }
}
