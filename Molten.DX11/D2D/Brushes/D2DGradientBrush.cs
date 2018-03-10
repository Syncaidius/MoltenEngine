using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class D2DGradientBrush : D2DBrush
    {
        Color _color;
        LinearGradientBrush _cBrush;
        LinearGradientBrushProperties _linearProperties;
        GradientStopCollection _stopCollection;
        GradientStop[] _stops;
        Gamma _gamma = Gamma.Linear;
        ExtendMode _extendMode = ExtendMode.Clamp;

        Vector2F _start;
        Vector2F _end;

        public D2DGradientBrush(D2DSurface surface)
            : base(surface)
        {
            _surface = surface;
            _color = Color.White;
            _start = new Vector2F();
            _end = new Vector2F();
        }

        protected override void OnRefreshBrush()
        {
            if (_surface.UnderlyingTarget != null)
            {
                //create default stops if needed.
                if (_stops == null)
                {
                    //default stops
                    _stops = new GradientStop[2];
                    _stops[0] = new GradientStop()
                    {
                        Color = Color.White.ToApi(),
                        Position = 0,
                    };

                    _stops[1] = new GradientStop()
                    {
                        Color = Color.Black.ToApi(),
                        Position = 0,
                    };
                }

                //setup brush
                _stopCollection = new GradientStopCollection(_surface.UnderlyingTarget, _stops, _gamma, _extendMode);

                _cBrush = new LinearGradientBrush(_surface.UnderlyingTarget, _linearProperties, _properties, _stopCollection);
                _cBrush.StartPoint = _start.ToApi();
                _cBrush.EndPoint = _end.ToApi();

                _brush = _cBrush;
            }
        }

        /// <summary>Gets the number of gradient stops.</summary>
        public int StopCount { get { return _stops.Length; } }

        /// <summary>Gets or sets the list of gradient stops used by the brush.</summary>
        public GradientStop[] Stops
        {
            get { return _stops; }
            set
            {
                _stops = value;

                //update the brush
                RefreshBrush();
            }
        }

        public Vector2F StartPoint
        {
            get { return _start; }
            set
            {
                _start = value;

                if (_cBrush != null)
                    _cBrush.StartPoint = _start.ToApi();
            }
        }

        public Vector2F EndPoint
        {
            get { return _end; }
            set
            {
                _end = value;

                if (_cBrush != null)
                    _cBrush.EndPoint = _end.ToApi();
            }
        }
    }
}