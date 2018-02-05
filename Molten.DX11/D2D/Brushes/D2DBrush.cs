using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal abstract class D2DBrush : EngineObject
    {
        protected D2DSurface _surface;
        protected Brush _brush;
        protected BrushProperties _properties;

        public D2DBrush(D2DSurface surface)
        {
            _surface = surface;
            _properties = new BrushProperties()
            {
                Opacity = 1.0f,
            };
            _surface.RegisterBrush(this);
        }

        /// <summary>(re)Creates the D2D brush object.</summary>
        internal void RefreshBrush()
        {
            //dispose of old brush.
            if (_brush != null)
                _brush.Dispose();

            if(_surface.UnderlyingTarget != null)
                OnRefreshBrush();
        }

        protected abstract void OnRefreshBrush();

        protected override void OnDispose()
        {
            _brush?.Dispose();
        }

        /// <summary>Gets the underlying D2D brush object.</summary>
        public Brush UnderlyingBrush { get { return _brush; } }

        /// <summary>Gets or sets the opacity of the brush.</summary>
        public float Opacity
        {
            get { return _properties.Opacity; }
            set
            {
                _properties.Opacity = value;

                if (_brush != null)
                    _brush.Opacity = value;
            }
        }

        /// <summary>Gets whether or not the brush is ready to be used.</summary>
        public bool IsValid { get { return _brush != null; } }
    }
}
