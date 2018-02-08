using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Molten.Rendering;
using System.Threading;
using Molten.Collections;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>A line graph for presenting sets of recorded values.</summary>
    public class UILineGraph : UIComponent
    {
        const int DEFAULT_PB_SIZE = 1024;
        static PrimitiveBatch _pb;
        static int _pbReferences;
        static Matrix _mIdentity = Matrix.Identity;

        /// <summary>Invoked when a <see cref="DataSet"/> was added/removed from the graph.</summary>
        public ObjectHandler<UILineGraph> OnSetsChanged;

        public class DataSet
        {
            internal ThreadedList<float> Points = new ThreadedList<float>();

            /// <summary>The color of the plotted graph lines for this data set.</summary>
            public Color Color = Color.White;

            public bool IsVisible = true;

            public float Highest { get; private set; }

            public float Average { get; private set; }

            public string Name { get; private set; }

            public DataSet(string name)
            {
                Name = name;
            }

            public void Plot(float value)
            {
                Points.Add(value);
            }

            public void CalcHighAverage()
            {
                Highest = 0;
                Average = 0;
                Points.ForInterlock(0, 1, (id, v) =>
                {
                    if (v > Highest)
                        Highest = v;

                    Average += v;
                    return false;
                });

                Average /= Points.Count;
            }
        }

        ThreadedList<DataSet> _data;

        int _maxPoints = 50;
        int _fontSize;
        string _title = "";
        string _fontName;
        SpriteFont _font;
        Color _axisColor = Color.DarkGray;
        Color _labelColor = Color.White;
        Color _refColor = new Color(100, 100, 100, 255);

        double _startValue = 0;
        double _range = 1;
        float _highest = 1;

        double _updateTime;
        double _updateInterval = 1000;

        Vector2 _axisXPos;
        Vector2 _axisYPos;
        Vector2 _graphPos;

        Vector2 _xStartSize;
        Vector2 _xEndSize;
        Vector2 _xValuePos;

        string _axisXValStart;
        string _axisXValEnd;
        string _axisLabelX;
        string _axisLabelY;

        int _graphWidth;
        int _graphHeight;

        public UILineGraph(UISystem ui) : base(ui)
        {
            _data = new ThreadedList<DataSet>();
            
            _fontName = _ui.DefaultFontName;
            _fontSize = 14;
            GetFont();

            StartValue = 0;

            if (_pb == null)
                _pb = new PrimitiveBatch(ui.Engine, PrimitiveBatchTopology.LineList, DEFAULT_PB_SIZE);

            Interlocked.Increment(ref _pbReferences);
        }

        private void GetFont()
        {
            _font = SpriteFont.Create(_ui.Engine.GraphicsDevice, _fontName, _fontSize);
            OnUpdateBounds();
        }

        public void AddDataSet(DataSet set)
        {
            _data.Add(set);
            OnSetsChanged?.Invoke(this);
        }

        public void RemoveDataSet(DataSet set)
        {
            _data.Remove(set);
            OnSetsChanged?.Invoke(this);
        }

        private void UpdateHorizontalValues()
        {
            _axisXValStart = _startValue.ToString("N0");
            _axisXValEnd = (_startValue + _range).ToString("N0");

            _xStartSize = _font.MeasureString(_axisXValStart);
            _xEndSize = _font.MeasureString(_axisXValEnd);

            _xValuePos = new Vector2()
            {
                X = (int)(_clippingBounds.Right - _xEndSize.X),
                Y = _graphPos.Y + 5,
            };
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Vector2 axisXSize = _font.MeasureString(_axisLabelX);
            Vector2 axisYSize = _font.MeasureString(_axisLabelY);

            int labelXHeight = (int)axisXSize.Y + 10;
            int labelYWidth = (int)axisYSize.X + 10;
            _graphWidth = _clippingBounds.Width - labelYWidth;
            _graphHeight = _clippingBounds.Height - labelXHeight;
            _graphPos = _clippingBounds.BottomLeft + new Vector2(labelYWidth, -labelXHeight);


            _axisXPos = new Vector2()
            {
                X = _graphPos.X + (_graphWidth / 2) - (axisXSize.X / 2),
                Y = _graphPos.Y + 5,
            };

            _axisYPos = new Vector2()
            {
                X = _clippingBounds.X,
                Y = _graphPos.Y - (_graphHeight * 0.125f) - axisYSize.Y,
            };

            UpdateHorizontalValues();
        }

        protected override void OnUpdate(Schedule time)
        {
            double elapsed = time.ElapsedTime.TotalMilliseconds;
            _updateTime += elapsed;
            if (_updateTime >= _updateInterval)
            {
                _updateTime -= _updateInterval;
                _highest = 1;

                int delThreshold = _maxPoints + 1;

                // Handle removal of oldest point from each data set.
                _data.ForInterlock(0, 1, (id, set) =>
                {
                    if (set.Points.Count > delThreshold)
                        set.Points.RemoveRange(0, set.Points.Count - delThreshold);

                    set.CalcHighAverage();

                    // Track highest possible value
                    if (set.Highest > _highest)
                        _highest = set.Highest;

                    return false;
                });
            }
            base.OnUpdate(time);
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            base.OnRender(sb, proxy);

            // Draw axis
            Vector2 p1 = _graphPos;
            Vector2 p2 = _graphPos + new Vector2(0, -_graphHeight);
            _pb.AddLine(p1, p2, _axisColor);

            p2 = _graphPos + new Vector2(_graphWidth, 0);
            _pb.AddLine(p1, p2, _axisColor);

            p1 = _clippingBounds.TopRight + new IntVector2(0, _graphHeight);
            p2 = _clippingBounds.TopRight;
            _pb.AddLine(p1, p2, _axisColor);

            // Draw axis labels
            sb.DrawString(_font, _axisLabelX, _axisXPos, _labelColor);
            sb.DrawString(_font, _axisLabelY, _axisYPos, _labelColor);

            // Draw vertical axis values based on highest value
            Vector2 valPos;
            Vector2 tSize;
            string strV;
            for (int i = 0; i <= 4; i++)
            {
                float percent = i * 0.25f;
                float val = MathHelper.Lerp(0, _highest, percent);
                strV = val.ToString("N0");
                tSize = _font.MeasureString(strV);

                valPos.X = _graphPos.X - tSize.X - 5;
                valPos.Y = _graphPos.Y - (_graphHeight * percent) - (tSize.Y / 2);

                sb.DrawString(_font, strV, valPos, _labelColor);

                // Draw reference line
                p1 = new Vector2()
                {
                    X = _graphPos.X - 5,
                    Y = valPos.Y + (tSize.Y / 2),
                };

                p2 = p1;
                p2.X += _graphWidth + 5;
                _pb.AddLine(p1, p2, _refColor);
            }

            // Draw horizontal axis values based on set range.
            valPos = _xValuePos;
            sb.DrawString(_font, _axisXValEnd, valPos, _labelColor);
            valPos.X = _graphPos.X - (_xStartSize.X / 2);
            sb.DrawString(_font, _axisXValStart, valPos, _labelColor);

            // Plot data sets.
            _data.ForInterlock(0, 1, (id, set) =>
            {
                PlotSet(set);
                return false;
            });

            proxy.DrawBatch(sb, BatchSortMode.None, BlendingPreset.PreMultipliedAlpha, RenderLayer.AfterPostProcess);
            proxy.DrawBatch(_pb, _mIdentity, BlendingPreset.Default, DepthStencilPreset.ZDisabled);
        }

        private void PlotSet(DataSet set)
        {
            if (set.Points.Count == 0 || !set.IsVisible)
                return;

            //Pre-calculate first point
            double percent = (float)(set.Points[0] / _highest);
            float xOffset = 0;
            float yOffset = (float)_graphHeight * (float)percent;
            float xIncrement = (float)_graphWidth / (_maxPoints - 1);

            Vector2 prev = new Vector2()
            {
                X = xOffset,
                Y = -yOffset,
            };

            prev += _graphPos;

            // Plot the rest of the points.
            set.Points.ForInterlock(1, 1, _maxPoints, (id, v) =>
            {
                percent = v / _highest;
                yOffset = (float)_graphHeight * (float)percent;
                xOffset = xIncrement * id;

                Vector2 p = new Vector2()
                {
                    X = xOffset,
                    Y = -yOffset, // Higher values are a lower Y value (up goes toward 0),
                };

                p += _graphPos;

                _pb.AddLine(prev, p, set.Color);
                prev = p;
                return false;
            });
        }

        protected override void OnDispose()
        {
            int old = Interlocked.Decrement(ref _pbReferences);
            if (old == 1)
            {
                _pb.Dispose();
                _pb = null;
            }

            base.OnDispose();
        }

        /// <summary>Gets the thread-safe list containing all of the data sets.</summary>
        internal ThreadedList<DataSet> Sets { get { return _data; } }

        /// <summary>Gets or sets the maximum number of points on the graph.</summary>
        public int MaxPoints
        {
            get { return _maxPoints; }
            set { _maxPoints = value; }
        }

        /// <summary>Gets or sets the title of the line graph.</summary>
        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [DataMember]
        public string Font
        {
            get { return _font.FontName; }
            set
            {
                _fontName = value;
                GetFont();
            }
        }

        [DataMember]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                GetFont();
            }
        }

        [DataMember]
        /// <summary>Gets or sets the graph axis color.</summary>
        public Color AxisColor
        {
            get { return _axisColor; }
            set { _axisColor = value; }
        }

        [DataMember]
        /// <summary>Gets or sets the color of the graph reference lines.</summary>
        public Color ReferenceLineColor
        {
            get { return _refColor; }
            set { _refColor = value; }
        }

        [DataMember]
        /// <summary>Gets or sets the graph label color.</summary>
        public Color LabelColor
        {
            get { return _labelColor; }
            set
            {
                _labelColor = value;

            }
        }

        [DataMember]
        /// <summary>Gets or sets the label for the X (horizontal) axis.</summary>
        public string AxisXLabel
        {
            get { return _axisLabelX; }
            set
            {
                _axisLabelX = value;
                OnUpdateBounds();
            }
        }

        [DataMember]
        /// <summary>Gets or sets the label for the Y (vertical) axis.</summary>
        public string AxisYLabel
        {
            get { return _axisLabelY; }
            set
            {
                _axisLabelY = value;
                OnUpdateBounds();
            }
        }

        [DataMember]
        /// <summary>Gets or sets the start value along the horizontal (X) axis. The rest are calculated based on <see cref="ValueRange"/></summary>
        public double StartValue
        {
            get { return _startValue; }
            set
            {
                _startValue = value;
                UpdateHorizontalValues();
            }
        }

        [DataMember]
        /// <summary>Gets or sets the start value along the horizontal (X) axis. The rest are calculated based on <see cref="ValueRange"/></summary>
        public double ValueRange
        {
            get { return _range; }
            set
            {
                _range = value;
                UpdateHorizontalValues();
            }
        }
    }
}
