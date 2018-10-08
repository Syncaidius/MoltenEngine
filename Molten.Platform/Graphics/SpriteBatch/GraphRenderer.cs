using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphRenderer
    {
        int _maxPoints;
        int _populated;
        double[] _values;
        Interlocker _interlocker;
        Vector2F _pixelScale;
        Vector2F[] _points;
        RectangleF _bounds;

        public void MaxPoints(int maxPoints)
        {
            _interlocker = new Interlocker();
            _maxPoints = maxPoints;
            _values = new double[_maxPoints];
            _points = new Vector2F[_maxPoints];
            _bounds = new RectangleF(0, 0, 100, 100);
        }

        public void Add(double value)
        {
            _interlocker.Lock(() =>
            {
                if (_populated == _values.Length) {
                    Array.Copy(_values, 1, _values, 0, _maxPoints - 1);
                    _populated--;
                }

                _values[_populated++] = value;
            });
        }

        public void Clear()
        {
            _populated = 0;
        }

        public void Render(SpriteBatcher sb, SpriteFont font)
        {
            double highest = 0;
            double lowest = 0;
            double val = 0;
            double average = 0;
            float range = 0;
            Vector2F pixelScale;
            RectangleF plotArea = _bounds;

            _interlocker.Lock(() =>
            {
                for (int i = 0; i < _populated; i++)
                {
                    val = _values[i];
                    lowest = val < lowest ? val : lowest;
                    highest = val > highest ? val : highest;
                    average += val;
                }
            });

            average /= _populated;
            range = (float)(highest - lowest);
            pixelScale = new Vector2F()
            {
                X = _bounds.Width / range,
                Y = _bounds.Height / range,
            };


            _interlocker.Lock(() =>
            {
                for(int i = 0; i < _populated; i++)
                {
                    val = _values[i] - lowest;
                    _points[i] = new Vector2F()
                    {
                        X = plotArea.Left + (float)(pixelScale.X * val),
                        Y = plotArea.Top + (float)(pixelScale.Y * val),
                    };
                }

                sb.DrawLinePath(_points, 0, _populated, LineColor, 1);
            });
        }

        public RectangleF Bounds
        {
            get => _bounds;
            set => _bounds = value;
        }

        public string Title { get; set; } = "Untitled Graph";

        public string AxisLabelX { get; set; } = "X Axis";

        public string AxisLabelY { get; set; } = "Y Axis";

        public bool ShowAverageLine { get; set; } = true;

        public Color LineColor { get; set; } = Color.White;

        public Color AverageLineColor { get; set; } = Color.Yellow;
    }
}
