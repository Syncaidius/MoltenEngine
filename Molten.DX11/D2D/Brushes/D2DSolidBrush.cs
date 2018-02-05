using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class D2DSolidBrush : D2DBrush
    {
        Color _color;
        SolidColorBrush _cBrush;

        internal D2DSolidBrush(D2DSurface surface)
            : base(surface)
        {
            _surface = surface;
            _color = Color.White;
        }

        protected override void OnRefreshBrush()
        {
            _cBrush = new SolidColorBrush(_surface.UnderlyingTarget, _color.ToApi(), _properties);
            _brush = _cBrush;
        }

        /// <summary>Gets or sets the color of the brush.</summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;

                if(_cBrush != null)
                    _cBrush.Color = value.ToApi();
            }
        }
    }
}