using Molten.Graphics;

namespace Molten.UI
{
    public class UIScrollBar : UIElement
    {
        public event UIElementHandler<UIScrollBar> ValueChanged;

        UIElementFlowDirection _direction;
        UIButton _btnDecrease;
        UIButton _btnIncrease;
        CornerInfo _corners = new CornerInfo(0);

        Rectangle _bgBounds;
        Rectangle _barBounds;
        float _minValue = 0;
        float _maxValue = 50;
        float _value = 0;
        float _increment = 1;
        bool _barPressed;

        RectStyle _style = new RectStyle(
            new Color(0, 109, 155, 200),
            new Color(52, 189, 235, 255),
            new Thickness(2, 0));

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _btnDecrease = BaseElements.Add<UIButton>();
            _btnDecrease.HorizontalAlign = UIHorizonalAlignment.Center;
            _btnDecrease.VerticalAlign = UIVerticalAlignment.Center;

            _btnIncrease = BaseElements.Add<UIButton>();
            _btnIncrease.HorizontalAlign = UIHorizonalAlignment.Center;
            _btnIncrease.VerticalAlign = UIVerticalAlignment.Center;

            RefreshButtons();

            _btnDecrease.Pressed += _btnDecrease_Pressed;
            _btnIncrease.Pressed += _btnIncrease_Pressed;
        }

        private void RefreshButtons()
        {
            if (Direction == UIElementFlowDirection.Vertical)
            {
                _btnDecrease.CornerRadius = new CornerInfo(_corners.TopLeft, _corners.TopRight, 0, 0);
                _btnIncrease.CornerRadius = new CornerInfo(0, 0, _corners.BottomRight, _corners.BottomLeft);
                _btnDecrease.Text = "^";
                _btnIncrease.Text = "v";
            }
            else // Horizontal
            {
                _btnDecrease.CornerRadius = new CornerInfo(_corners.TopLeft, 0, 0, _corners.BottomLeft);
                _btnIncrease.CornerRadius = new CornerInfo(0, _corners.TopRight, _corners.BottomRight, 0);
                _btnDecrease.Text = "<";
                _btnIncrease.Text = ">";
            }
        }

        protected override void ApplyTheme()
        {
            base.ApplyTheme();
            RefreshButtons();
        }

        private void _btnIncrease_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Value += _increment;
        }

        private void _btnDecrease_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Value -= _increment;
        }

        /// <summary>
        /// A helper method for setting <see cref="MinValue"/>, <see cref="MaxValue"/> and <see cref="Increment"/> with only one bounds update.
        /// </summary>
        /// <param name="min">The minimum value of the current <see cref="UIScrollBar"/>.</param>
        /// <param name="max">The maximum value of the current <see cref="UIScrollBar"/>.</param>
        /// <param name="increment">The increment of the current <see cref="UIScrollBar"/>.</param>
        public void Set(int min, int max, int increment)
        {
            _minValue = min;
            _maxValue = max;
            _increment = increment;
            UpdateBarBounds();
        }

        public override void OnPressed(CameraInputTracker tracker)
        {
            base.OnPressed(tracker);

            if (_barBounds.Contains((Vector2I)tracker.Position))
            {
                _barPressed = true;
            }
            else if(_bgBounds.Contains((Vector2I)tracker.Position))
            {
                if(Direction == UIElementFlowDirection.Vertical)
                {
                    if (tracker.Position.Y < _barBounds.Top)
                        Value -= Increment;
                    else
                        Value += Increment;
                }
                else
                {
                    if (tracker.Position.X < _barBounds.Left)
                        Value -= Increment;
                    else
                        Value += Increment;
                }
            }
        }

        public override void OnDragged(CameraInputTracker tracker)
        {
            base.OnDragged(tracker);

            if(_barPressed)
            {
                if (Direction == UIElementFlowDirection.Vertical)
                    Value += tracker.Delta.Y;
                else
                    Value += tracker.Delta.X;
            }
        }

        public override void OnReleased(CameraInputTracker tracker, bool releasedOutside)
        {
            base.OnReleased(tracker, releasedOutside);
            _barPressed = false;
        }

        protected override void OnPreUpdateLayerBounds()
        {
            base.OnPreUpdateLayerBounds();
            int buttonSize = Math.Min(LocalBounds.Width, LocalBounds.Height);
            Rectangle buttonBounds = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = buttonSize,
                Height = buttonSize,
            };

            _btnDecrease.LocalBounds = buttonBounds;
            Rectangle gBounds = GlobalBounds;

            if (Direction == UIElementFlowDirection.Vertical)
            {
                buttonBounds.Y = LocalBounds.Height - buttonSize;
                _btnIncrease.LocalBounds = buttonBounds;

                _bgBounds = new Rectangle()
                {
                    X = gBounds.X,
                    Y = gBounds.Y + buttonSize,
                    Width = gBounds.Width,
                    Height = LocalBounds.Height - (buttonSize * 2)
                };
            }
            else
            {
                buttonBounds.X = LocalBounds.Width - buttonSize;
                _btnIncrease.LocalBounds = buttonBounds;

                _bgBounds = new Rectangle()
                {
                    X = gBounds.X + buttonSize,
                    Y = gBounds.Y,
                    Width = LocalBounds.Width - (buttonSize * 2),
                    Height = gBounds.Height
                };
            }

            UpdateBarBounds();
        }

        private void UpdateBarBounds()
        {
            _barBounds = _bgBounds;

            // First calculate the local size of the bar
            float range = _maxValue - _minValue;
            float barStep = range / Increment;
            float percentOfRange = (_maxValue - (_maxValue - _value)) / range;

            if (Direction == UIElementFlowDirection.Vertical) 
            {
                _barBounds.Inflate(-((int)BorderThickness + 1), 0);
                float barSize = _barBounds.Height / barStep;

                _barBounds.Height = (int)barSize;
                _barBounds.Y += (int)Math.Ceiling((_bgBounds.Height - barSize) * percentOfRange);
            }
            else
            {
                _barBounds.Inflate(0, -((int)BorderThickness + 1));
                float barSize = _barBounds.Width / barStep;
                _barBounds.Width = (int)barSize;
                _barBounds.X += (int)Math.Ceiling((_bgBounds.Width - barSize) * percentOfRange);
            }
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);
            sb.DrawRect((RectangleF)_bgBounds, ref _style);
            sb.DrawRect((RectangleF)_barBounds, _style.BorderColor);
        }

        /// <summary>
        /// Gets or sets the direction of the current <see cref="UIScrollBar"/>.
        /// </summary>
        public UIElementFlowDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    RefreshButtons();
                    UpdateBounds();
                }
            }
        }

        [UIThemeMember]
        public Color BackgroundColor
        {
            get => _style.FillColor;
            set
            {
                _style.FillColor = value;
                _btnDecrease.FillColor = _style.FillColor;
                _btnIncrease.FillColor = _style.FillColor;
            }
        }

        [UIThemeMember]
        public Color BorderColor
        {
            get => _style.BorderColor;
            set
            {
                _style.BorderColor = value;
                _btnDecrease.BorderColor = _style.BorderColor;
                _btnIncrease.BorderColor = _style.BorderColor;
            }
        }        

        [UIThemeMember]
        public float BorderThickness
        {
            get => _style.BorderThickness.Left;
            set
            {
                if(_style.BorderThickness.Left != value)
                    _style.BorderThickness = new Thickness(value, 0);
            }
        }

        [UIThemeMember]
        public CornerInfo Corners
        {
            get => _corners;
            set
            {
                _corners = value;
                RefreshButtons();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value represented by the current <see cref="UIScrollBar"/>.
        /// </summary>
        public float MinValue
        {
            get => _minValue;
            set
            {
                if(_minValue != value)
                {
                    _minValue = value;
                    _value = Math.Max(_minValue, value);

                    UpdateBarBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value represented by the current <see cref="UIScrollBar"/>.
        /// </summary>
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if(_maxValue != value)
                {
                    _maxValue = value;
                    _value = Math.Min(_maxValue, _value);

                    UpdateBarBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the current <see cref="UIScrollBar"/>. The value will automatically be clamped between <see cref="MinValue"/> to <see cref="MaxValue"/>.
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                value = float.Clamp(value, _minValue, _maxValue);

                if (_value != value)
                {
                    _value = value;
                    UpdateBarBounds();
                    ValueChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the increment that <see cref="Value"/> increases or decreases when the currrent <see cref="UIScrollBar"/> is scrolled.
        /// </summary>
        public float Increment
        {
            get => _increment;
            set
            {
                if(_increment != value)
                {
                    _increment = value;
                    UpdateBarBounds();
                }
            }
        }
    }
}
