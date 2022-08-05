using Molten.Data;
using Molten.Graphics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public class UILineGraph : UIGraphBase
    {
        struct GraphMeasurements
        {
            public double Low;

            public double High;

            public double Average;

            public double XPerValue;
            public double YPerUnit;

            public int DataPointCount;

            public GraphMeasurements()
            {
                Low = double.MaxValue;
                High = double.MinValue;
                Average = 0;
                XPerValue = 0;
                YPerUnit = 0;
                DataPointCount = 0;
            }

            public double GetRange()
            {
                return High - Low;
            }
        }

        [UIThemeMember]
        public RectStyle BackgroundStyle { get; set; } = new RectStyle()
        {
            BorderColor = new Color(52, 189, 235, 255),
            BorderThickness = new RectBorderThickness(2,0, 0, 2),
            FillColor = new Color(0, 109, 155, 200),
        };

        /// <summary>
        /// Gets the style used for drawing the plotted data lines of the current <see cref="UILineGraph"/>.
        /// </summary>
        [UIThemeMember]
        public LineStyle DataLineStyle = new LineStyle(Color.White, 4, 0.1f);

        /// <summary>
        /// Gets the style used for drawing the average value line of the current <see cref="UILineGraph"/>.
        /// </summary>
        [UIThemeMember]
        public LineStyle AverageLineStyle = new LineStyle(Color.Yellow, 1, 1);

        /// <summary>
        /// Gets or sets whether the average line is shown.
        /// </summary>
        [UIThemeMember]
        public bool ShowAverageLine { get; set; } = true;

        UILabel _labelTitle;
        UILabel _labelXAxis;
        UILabel _labelYAxis;

        List<GraphDataSet> _datasets;

        RectangleF _plotArea;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            _datasets = new List<GraphDataSet>();

            _labelTitle = CompoundElements.Add<UILabel>();
            _labelTitle.Text = "Line Graph";
            _labelTitle.OnMeasurementChanged += Label_OnMeasurementChanged;

            _labelXAxis = CompoundElements.Add<UILabel>();
            _labelXAxis.Text = "X Axis";
            _labelXAxis.OnMeasurementChanged += Label_OnMeasurementChanged;

            _labelYAxis = CompoundElements.Add<UILabel>();
            _labelYAxis.Text = "Y Axis";
            _labelYAxis.OnMeasurementChanged += Label_OnMeasurementChanged;
        }

        public void AddDataSet(GraphDataSet dataset)
        {
            _datasets.Add(dataset);
        }

        private void Label_OnMeasurementChanged(UILabel obj)
        {
            OnUpdateBounds();
        }

        protected override void ApplyTheme()
        {
            base.ApplyTheme();

            _labelTitle.HorizontalAlign = UIHorizonalAlignment.Center;
            _labelXAxis.HorizontalAlign = UIHorizonalAlignment.Center;
            _labelYAxis.HorizontalAlign = UIHorizonalAlignment.Right;
            _labelYAxis.VerticalAlign = UIVerticalAlignment.Center;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gb = GlobalBounds;

            float labelSeparation = 5;
            float titleSpace = _labelTitle.MeasuredSize.Y + labelSeparation;
            float xAxisSpace = _labelXAxis.MeasuredSize.Y + labelSeparation;

            int titleHeight = (int)Math.Ceiling(_labelTitle.MeasuredSize.Y);
            int xAxisHeight = (int)Math.Ceiling(_labelXAxis.MeasuredSize.Y);
            int yAxisHeight = (int)Math.Ceiling(_labelYAxis.MeasuredSize.Y);

            _labelTitle.LocalBounds = new Rectangle(0,0, gb.Width, titleHeight);
            _labelXAxis.LocalBounds = new Rectangle(0, gb.Height - xAxisHeight, gb.Width, xAxisHeight);

            _plotArea = new RectangleF()
            {
                X = gb.X,
                Y = gb.Y + titleSpace,
                Width = gb.Width,
                Height = gb.Height - titleSpace - xAxisSpace
            };
        }

        protected override void OnRenderSelf(SpriteBatcher sb)
        {
            sb.DrawRect(_plotArea, BackgroundStyle);

            GraphMeasurements gm = new GraphMeasurements();

            // Take some measurements
            for (int i = 0; i < _datasets.Count; i++)
            {
                GraphDataSet set = _datasets[i];
                set.Calculate();

                if (set.LowestValue < gm.Low)
                    gm.Low = set.LowestValue;

                if (set.HighestValue > gm.High)
                    gm.High = set.HighestValue;

                if (set.Capacity > gm.DataPointCount)
                    gm.DataPointCount = set.Capacity;

                gm.Average += set.AverageValue;
            }

            gm.Average /= _datasets.Count;
            gm.XPerValue = _plotArea.Width / gm.DataPointCount;
            gm.YPerUnit = _plotArea.Height / gm.GetRange();

            for (int i = 0; i < _datasets.Count; i++)
            {
                GraphDataSet set = _datasets[i];

                if (set.NextValueIndex < set.Count)
                {
                    int p1Len = set.Count - set.NextValueIndex;
                    int p2Len = set.NextValueIndex;

                    DrawRange(sb, set, ref gm, set.GetSpan(set.NextValueIndex, p1Len), 0);
                    DrawConnectingLine(sb, set, ref gm, p1Len - 1);
                    DrawRange(sb, set, ref gm, set.GetSpan(0, p2Len), p1Len);

                }
                else
                {
                    DrawRange(sb, set, ref gm, set.GetSpan(0, set.Count), 0);
                }
            }


            if (ShowAverageLine)
            {
                double avgLocalValue = gm.Average - gm.Low;
                Vector2F avg1 = new Vector2F(_plotArea.X, _plotArea.Bottom - (float)(gm.YPerUnit * avgLocalValue));
                Vector2F avg2 = new Vector2F(_plotArea.Right, avg1.Y);
                sb.DrawLine(avg1, avg2, ref AverageLineStyle);
            }

            base.OnRenderSelf(sb);
        }

        /// <summary>
        /// Draws a connecting line between first range end-point and second range start-point
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="set"></param>
        /// <param name="gm"></param>
        private unsafe void DrawConnectingLine(SpriteBatcher sb, GraphDataSet set, ref GraphMeasurements gm, int startIndex)
        {
            DataLineStyle.Color1 = set.KeyColor;
            DataLineStyle.Color2 = set.KeyColor;

            double* points = stackalloc double[2];
            points[0] = set[set.Count - 1];
            points[1] = set[0];

            DrawRange(sb, set, ref gm, new Span<double>(points, 2), startIndex);
        }

        private void DrawRange(SpriteBatcher sb, GraphDataSet set, ref GraphMeasurements gm, Span<double> range, int startIndex)
        {
            Vector2F pPrev = new Vector2F();

            for(int i = 0; i < range.Length; i++)
            {
                // Get the value, local to the range covered by the graph, within the lowest to highest values.
                double graphLocalValue = range[i] - set.LowestValue;

                Vector2F p = new Vector2F()
                {
                    X = _plotArea.Left + (int)(gm.XPerValue * (i + startIndex)),
                    Y = _plotArea.Bottom - (int)(gm.YPerUnit * graphLocalValue)
                };

                if (i > 0)
                {
                    DataLineStyle.Color1 = set.KeyColor;
                    DataLineStyle.Color2 = set.KeyColor;

                    sb.DrawLine(pPrev, p, ref DataLineStyle);
                }

                pPrev = p;
            }
        }
    }
}
