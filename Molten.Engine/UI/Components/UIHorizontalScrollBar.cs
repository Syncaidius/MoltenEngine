using Molten.Graphics;
using Molten.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>A horizontal scrollbar control.</summary>
    public class UIHorizontalScrollBar : UICompoundComponent
    {
        int _min = 0;
        int _max = 100;
        int _value = 0;
        int _scrollSpeed = 5;

        UIButton _leftButton;
        UIButton _rightButton;

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

        public event UIComponentHandler<UIHorizontalScrollBar> OnScroll;

        public UIHorizontalScrollBar(Engine engine) : base(engine)
        {
            _colorBackground = new Color(20, 40, 60, 255);
            _colorBar = new Color(150, 150, 200, 255);
            _colorBarClicked = new Color(200, 200, 255, 255);
            _colorBarHover = new Color(180, 180, 210, 255);

            _leftButton = new UIButton(engine);
            _leftButton.Text.Text = "◀";

            _rightButton = new UIButton(engine);
            _rightButton.Text.Text = "▶";

            AddPart(_leftButton);
            AddPart(_rightButton);

            _leftButton.OnClickEnded += _leftButton_OnClickCompleted;
            _rightButton.OnClickEnded += _rightButton_OnClickCompleted;

            OnClickStarted += UIHorizontalScrollBar_OnClickStarted;
            OnDrag += UIHorizontalScrollBar_OnDrag;
            OnHover += UIHorizontalScrollBar_OnHover;
            OnLeave += UIHorizontalScrollBar_OnLeave;
            OnClickEndedOutside += UIHorizontalScrollBar_OnPressCompletedOutside;
        }

        void UIHorizontalScrollBar_OnLeave(UIEventData<MouseButton> data)
        {
            _barHover = false;
        }

        void UIHorizontalScrollBar_OnHover(UIEventData<MouseButton> data)
        {
            _barHover = _barBounds.Contains(data.Position);
        }

        void UIHorizontalScrollBar_OnPressCompletedOutside(UIEventData<MouseButton> data)
        {
            _barClicked = false;
        }

        void UIHorizontalScrollBar_OnDrag(UIEventData<MouseButton> data)
        {
            if (_barClicked)
            {
                float dif = data.Position.X - _lastDragPos;
                float total = _jumpBounds.Width - _barBounds.Width;
                float difPercent = dif / total;
                int scrollAmount = (int)Math.Floor((float)_max * difPercent);

                Scroll(scrollAmount);

                _lastDragPos = data.Position.X;
            }
        }

        void UIHorizontalScrollBar_OnClickStarted(UIEventData<MouseButton> data)
        {
            _barClicked = _barBounds.Contains(data.Position);

            // Check if the area underneath the bar (the empty area) was clicked.
            if (!_barClicked)
            {
                if (_jumpBounds.Contains(data.Position))
                {
                    float dif = data.Position.X - _barBounds.X;
                    Scroll((int)(dif * _pixelsPerUnit));
                }
            }
            else
            {
                _lastDragPos = data.Position.X;
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

        public void Scroll(int amount)
        {
            _value = MathHelper.Clamp(_value + amount, _min, _max);
            RefreshBar();

            if (OnScroll != null)
                OnScroll(this);
        }

        private void RefreshBar()
        {
            int buttonWidth = (_globalBounds.Height);

            float viewWidth = _globalBounds.Width - (buttonWidth * 2);
            float range = _max - _min;
            float percent = Math.Min(1.0f, _barRange / range);
            float barSize = viewWidth * percent;

            _pixelsPerUnit = range / viewWidth;

            // Clickable area that allows the user to jump the scroll bar to a specific location.
            _jumpBounds = new Rectangle()
            {
                X = _globalBounds.X + buttonWidth,
                Y = _globalBounds.Y,
                Width = (int)viewWidth,
                Height = _globalBounds.Height,
            };

            // Calculate bar bounds
            viewWidth -= barSize;
            float scrollPercent = _value / range;
            float xPos = viewWidth * scrollPercent;
            xPos += buttonWidth;

            _barBounds = new Rectangle()
            {
                X = (int)xPos + _globalBounds.X,
                Y = _globalBounds.Y,
                Width = (int)barSize,
                Height = _globalBounds.Height,
            };
            _barBounds.Inflate(-_barPadding, -1);
        }

        protected override void OnUpdateBounds()
        {
            int buttonWidth = (_globalBounds.Height);
            float viewWidth = _globalBounds.Width - (buttonWidth * 2);

            // Clickable area that allows the user to jump the scroll bar to a specific location.
            _jumpBounds = new Rectangle()
            {
                X = _globalBounds.X + buttonWidth,
                Y = _globalBounds.Y,
                Width = (int)viewWidth,
                Height = _globalBounds.Height,
            };


            _leftButton.LocalBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Y,
                Width = buttonWidth,
                Height = _globalBounds.Height,
            };

            _rightButton.LocalBounds = new Rectangle()
            {
                X = _globalBounds.Right - buttonWidth,
                Y = _globalBounds.Y,
                Width = buttonWidth,
                Height = _globalBounds.Height,
            };

            RefreshBar();
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
        }

        protected override void OnRender(ISpriteBatch sb)
        {
            sb.Draw(_globalBounds, _colorBackground);

            if (_barClicked)
                sb.Draw(_barBounds, _colorBarClicked);
            else if (_barHover)
                sb.Draw(_barBounds, _colorBarHover);
            else
                sb.Draw(_barBounds, _colorBar);

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
                    OnScroll?.Invoke(this);
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
                    OnScroll?.Invoke(this);
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

        /// <summary>Gets the left button</summary>
        [Category("Parts")]
        [DisplayName("Left Button")]
        [ExpandablePropertyAttribute]
        [DataMember]
        public UIButton LeftButton
        {
            get { return _leftButton; }
        }

        /// <summary>Gets the right button</summary>
        [Category("Parts")]
        [DisplayName("Right Button")]
        [ExpandablePropertyAttribute]
        [DataMember]
        public UIButton Rightbutton
        {
            get { return _rightButton; }
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
