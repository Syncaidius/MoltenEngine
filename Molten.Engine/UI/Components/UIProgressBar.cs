using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    [DataContract]
    public class UIProgressBar : UIComponent
    {
        UIProgressBarStyle _style;
        Color _bgColor;
        Color _barColor;

        int _barPadding = 0;
        int _segmentWidth = 10;
        int _segmentSpacing = 1;

        int _minValue = 0;
        int _maxValue = 100;
        int _value = 0;
        bool _showText = false;
        bool _showValues = false;

        int _segmentCount;
        UIRenderedText _text;

        Rectangle _barBounds;

        public UIProgressBar(Engine engine) : base(engine)
        {
            _bgColor = new Color(100, 100, 100, 255);
            _barColor = new Color(0, 255, 0, 255);
            _text = new UIRenderedText(engine);

            _style = UIProgressBarStyle.Solid;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            _text.Bounds = _globalBounds;
            CalculateBar();
            CalculateText();
        }

        private void CalculateBar()
        {
            _barBounds = _globalBounds;
            _barBounds.Inflate(-_barPadding, -_barPadding);

            // Move value into 0 - X range. 
            // Calculate max value based on the value range, not just the max value.
            int paddedWidth = _barPadding * 2;
            int range = _maxValue - _minValue;
            int barValue = _value + _minValue;

            float percent = (float)Math.Min((float)barValue / (float)range, 1.0f);
            _barBounds.Width = (int)(_barBounds.Width * percent);

            _segmentCount = (int)Math.Ceiling((float)_barBounds.Width / (float)_segmentWidth);
        }

        private void CalculateText()
        {
            _text.Text = "";

            if (_showText)
                _text.Text = _text.Text;

            if (_showValues)
                _text.Text += _value + " / " + _maxValue;
        }

        protected override void OnRender(ISpriteBatch sb)
        {
            sb.Draw(_globalBounds, _bgColor);

            // Draw bar
            if (_value != _minValue)
            {
                switch (_style)
                {
                    case UIProgressBarStyle.Solid:
                        sb.Draw(_barBounds, _barColor);
                        break;

                    case UIProgressBarStyle.Segmented:
                        int availableWidth = _barBounds.Width;
                        int increment = 0;

                        Rectangle segBounds = new Rectangle()
                        {
                            X = _barBounds.X,
                            Y = _barBounds.Y,
                            Width = _segmentWidth,
                            Height = _barBounds.Height,
                        };

                        // Draw segments
                        for (int i = 0; i < _segmentCount; i++)
                        {
                            segBounds.Width = (int)Math.Min(availableWidth, _segmentWidth);

                            sb.Draw(segBounds, _barColor);

                            increment = segBounds.Width + _segmentSpacing;
                            segBounds.X += increment;
                            availableWidth -= increment;

                            if (availableWidth <= 0)
                                break;
                        }
                        break;
                }
            }

            if (_showText || _showValues)
                _text.Draw(sb);

            base.OnRender(sb);
        }

        [Category("Appearance")]
        [DisplayName("Bar Style")]
        [DataMember]
        public UIProgressBarStyle BarStyle
        {
            get { return _style; }
            set { _style = value; }
        }

        [Category("Values")]
        [DisplayName("Minimum Value")]
        [DataMember]
        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                CalculateBar();
                CalculateText();
            }
        }

        [Category("Values")]
        [DisplayName("Maximum Value")]
        [DataMember]
        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                CalculateBar();
                CalculateText();
            }
        }

        [Category("Values")]
        [DisplayName("Value")]
        [DataMember]
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                CalculateBar();
                CalculateText();
            }
        }

        [Category("Appearance")]
        [DisplayName("Segment Width")]
        [DataMember]
        public int SegmentWidth
        {
            get { return _segmentWidth; }
            set { _segmentWidth = value; }
        }

        [Category("Appearance")]
        [DisplayName("Segment Spacing")]
        [DataMember]
        public int SegmentSpacing
        {
            get { return _segmentSpacing; }
            set { _segmentSpacing = value; }
        }

        [Category("Appearance")]
        [DisplayName("Bar Padding")]
        [DataMember]
        public int BarPadding
        {
            get { return _barPadding; }
            set
            {
                _barPadding = value;
                CalculateBar();
                CalculateText();
            }
        }

        [Category("Appearance")]
        [DisplayName("Background Color")]
        [DataMember]
        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }

        [Category("Appearance")]
        [DisplayName("Bar Color")]
        [DataMember]
        public Color BarColor
        {
            get { return _barColor; }
            set { _barColor = value; }
        }

        [ExpandablePropertyAttribute]
        [Category("Appearance")]
        [DisplayName("Text")]
        [DataMember]
        public UIRenderedText Text
        {
            get { return _text; }
        }

        [Category("Appearance")]
        [DisplayName("Show Text")]
        [DataMember]
        public bool ShowText
        {
            get { return _showText; }
            set
            {
                _showText = value;
                CalculateText();
            }
        }

        [Category("Appearance")]
        [DisplayName("Show Values")]
        [DataMember]
        public bool ShowValues
        {
            get { return _showValues; }
            set
            {
                _showValues = value;
                CalculateText();
            }
        }
    }

    public enum UIProgressBarStyle
    {
        Solid = 0,

        Segmented = 1,
    }
}
