using Molten.Graphics;
using Molten.IO;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>A horizontal scrollbar control.</summary>
    public class UIVerticalScrollBar : UICompoundComponent
    {
        int _min = 0;
        int _max = 100;
        int _value = 0;
        int _scrollSpeed = 5;

        UIButton _upButton;
        UIButton _downButton;

        Rectangle _jumpBounds;
        Rectangle _barBounds;
        int _barPadding = 2;
        int _barRange = 10;
        float _pixelsPerUnit = 0;

        Color _colorBackground;
        Color _colorBar;
        Color _colorBarHover;
        Color _colorBarClicked;

        bool _barClicked = false;
        bool _barHover = false;
        float _lastDragPos;

        public event UIComponentHandler<UIVerticalScrollBar> OnScroll;

        public UIVerticalScrollBar(Engine engine) : base(engine)
        {
            _colorBackground = new Color(20, 40, 60, 255);
            _colorBar = new Color(150, 150, 200, 255);
            _colorBarClicked = new Color(200, 200, 255, 255);
            _colorBarHover = new Color(180, 180, 210, 255);

            _upButton = new UIButton(engine);
            _upButton.Text.Text = "▲";

            _downButton = new UIButton(engine);
            _downButton.Text.Text = "▼";

            AddPart(_upButton);
            AddPart(_downButton);

            _upButton.OnClickEnded += _leftButton_OnClickCompleted;
            _downButton.OnClickEnded += _rightButton_OnClickCompleted;

            OnClickStarted += UIVerticalScrollBar_OnClickStarted;
            OnDrag += UIVerticalScrollBar_OnDrag;
            OnHover += UIVerticalScrollBar_OnHover;
            OnLeave += UIVerticalScrollBar_OnLeave;
            OnClickEndedOutside += UIHorizontalScrollBar_OnPressCompletedOutside;
        }

        void UIVerticalScrollBar_OnLeave(UIEventData<MouseButton> data)
        {
            _barHover = false;
        }

        void UIVerticalScrollBar_OnHover(UIEventData<MouseButton> data)
        {
            _barHover = _barBounds.Contains(data.Position);
        }

        void UIHorizontalScrollBar_OnPressCompletedOutside(UIEventData<MouseButton> data)
        {
            _barClicked = false;
        }

        void UIVerticalScrollBar_OnDrag(UIEventData<MouseButton> data)
        {
            if (_barClicked)
            {
                float dif = data.Position.Y - _lastDragPos;
                float total = _jumpBounds.Height - _barBounds.Height;
                float difPercent = dif / total;
                int scrollAmount = (int)Math.Floor((float)_max * difPercent);

                Scroll(scrollAmount);

                _lastDragPos = data.Position.Y;
            }
        }

        void UIVerticalScrollBar_OnClickStarted(UIEventData<MouseButton> data)
        {
            _barClicked = _barBounds.Contains(data.Position);

            // Check if the area underneath the bar (the empty area) was clicked.
            if (!_barClicked)
            {
                if (_jumpBounds.Contains(data.Position))
                {
                    float dif = data.Position.Y - _barBounds.Y;
                    Scroll((int)(dif * _pixelsPerUnit));
                }
            }
            else
            {
                _lastDragPos = data.Position.Y;
            }
        }

        void _rightButton_OnClickCompleted(UIEventData<MouseButton> data)
        {
            Scroll(_scrollSpeed);
        }

        void _leftButton_OnClickCompleted(UIEventData<MouseButton> data)
        {
            Scroll(-_scrollSpeed);
        }

        public void Scroll(int delta)
        {
            _value = MathHelper.Clamp(_value + delta, _min, _max);
            RefreshBar();

            if (OnScroll != null)
                OnScroll(this);
        }

        private void RefreshBar()
        {
            int buttonHeight = (_globalBounds.Width);

            _upButton.LocalBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Y,
                Width = _globalBounds.Width,
                Height = buttonHeight,
            };

            _downButton.LocalBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Bottom - buttonHeight,
                Width = _globalBounds.Width,
                Height = buttonHeight,
            };

            float viewHeight = _globalBounds.Height - (buttonHeight * 2);
            float range = _max - _min;
            float percent = Math.Min(1.0f, _barRange / range);
            float barSize = viewHeight * percent;

            _pixelsPerUnit = range / viewHeight;

            // Clickable area that allows the user to jump the scroll bar to a specific location.
            _jumpBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Y + buttonHeight,
                Width = _globalBounds.Width,
                Height = (int)viewHeight,
            };

            // Calculate bar bounds
            viewHeight -= barSize;
            float scrollPercent = _value / range;
            float yPos = viewHeight * scrollPercent;
            yPos += buttonHeight;

            _barBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = (int)yPos + _globalBounds.Y,
                Width = _globalBounds.Width,
                Height = (int)barSize,
            };
            _barBounds.Inflate(-_barPadding, -1);
        }

        protected override void OnUpdateBounds()
        {
            RefreshBar();
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
        }

        protected override void OnRender(SpriteBatch sb)
        {
            sb.DrawRect(_globalBounds, _colorBackground);

            if (_barClicked)
                sb.DrawRect(_barBounds, _colorBarClicked);
            else if (_barHover)
                sb.DrawRect(_barBounds, _colorBarHover);
            else
                sb.DrawRect(_barBounds, _colorBar);

            base.OnRender(sb);
        }

        /// <summary>Gets or sets the minimum X offset the scrollbar can provide.</summary>
        [Category("Settings")]
        [DisplayName("Min Value")]
        [DataMember]
        public int MinValue
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_value < _min)
                {
                    _value = _min;
                    if (OnScroll != null)
                        OnScroll(this);
                }

                RefreshBar();
            }
        }

        /// <summary>Gets or sets the maximum X offset the scrollbar can provide.</summary>
        [Category("Settings")]
        [DisplayName("Max Value")]
        [DataMember]
        public int MaxValue
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_value > _max)
                {
                    _value = _max;
                    if (OnScroll != null)
                        OnScroll(this);
                }

                RefreshBar();
            }
        }

        [Category("Settings")]
        [DisplayName("Value")]
        [DataMember]
        public int Value
        {
            get { return _value; }
            set
            {
                _value = MathHelper.Clamp(value, _min, _max);
                RefreshBar();
            }
        }

        [Category("Settings")]
        [DisplayName("Scroll Speed")]
        [DataMember]
        public int ScrollSpeed
        {
            get { return _scrollSpeed; }
            set { _scrollSpeed = value; }
        }

        /// <summary>Gets or sets the amount that the main bar represents.</summary>
        [Category("Settings")]
        [DisplayName("Bar Range")]
        [DataMember]
        public int BarRange
        {
            get { return _barRange; }
            set
            {
                _barRange = value;
                RefreshBar();
            }
        }


        /// <summary>Gets the up button</summary>
        [Category("Parts")]
        [DisplayName("Up Button")]
        [ExpandablePropertyAttribute]
        [DataMember]
        public UIButton UpButton
        {
            get { return _upButton; }
        }

        /// <summary>Gets the down button</summary>
        [Category("Parts")]
        [DisplayName("Down Button")]
        [ExpandablePropertyAttribute]
        [DataMember]
        public UIButton DownButton
        {
            get { return _downButton; }
        }

        [Category("Appearance")]
        [DisplayName("Background Color")]
        [DataMember]
        public Color BackgroundColor
        {
            get { return _colorBackground; }
            set { _colorBackground = value; }
        }

        [Category("Appearance")]
        [DisplayName("Bar Color")]
        [DataMember]
        public Color BarColor
        {
            get { return _colorBar; }
            set { _colorBar = value; }
        }

        [Category("Appearance")]
        [DisplayName("Bar Click Color")]
        [DataMember]
        public Color BarColorClicked
        {
            get { return _colorBarClicked; }
            set { _colorBarClicked = value; }
        }

        [Category("Appearance")]
        [DisplayName("Bar Hover Color")]
        [DataMember]
        public Color BarColorHovered
        {
            get { return _colorBarHover; }
            set { _colorBarHover = value; }
        }
    }
}
