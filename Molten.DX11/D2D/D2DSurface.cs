using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    using D2DRenderTarget = SharpDX.Direct2D1.RenderTarget;
    using Direct2DFactory = SharpDX.Direct2D1.Factory;
    using DirectWriteFactory = SharpDX.DirectWrite.Factory;
    using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

    internal delegate void D2DSurfaceHandler(GraphicsPipe pipe, D2DSurface surface);

    /// <summary>An interop surface for performing Direct2D and DirectWrite rendering operations on a Direct3D render surface.</summary>
    internal class D2DSurface : RenderSurface
    {
        Surface _surface;
        SurfaceDescription _desc;
        D2DRenderTarget _rt;
        Direct2DFactory _d2dFactory;
        DirectWriteFactory _dWriteFactory;

        List<D2DBrush> _brushes;
        D2DBrush _currentBrush;
        D2DSolidBrush _defaultBrush;

        bool _began;
        TextAntialiasMode _textAntiAlias = TextAntialiasMode.Default;

        internal event D2DSurfaceHandler OnRefreshed;

        /// <summary>Creates a new instance of Direct2DSurface.</summary>
        /// <param name="renderSurface">The Direct3D render surface to output all 2D operations.</param>
        internal D2DSurface(GraphicsDevice device, int width, int height) : base(device, width, height, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
        {
            _brushes = new List<D2DBrush>();
            _d2dFactory = device.Direct2D;
            _dWriteFactory = device.DirectWrite;
        }

        protected override SharpDX.Direct3D11.Resource CreateTextureInternal(bool resize)
        {
            ReleaseD2DSurface();

            SharpDX.Direct3D11.Resource res = base.CreateTextureInternal(resize);
            if (resize)
            {
                
            }
            else
            {
                _defaultBrush = new D2DSolidBrush(this);
                _currentBrush = _defaultBrush;
            }

            BuildD2DSurface(res);

            return res;
        }

        internal void RegisterBrush(D2DBrush brush)
        {
            if (_brushes.Contains(brush) == false)
            {
                _brushes.Add(brush);
                brush.RefreshBrush();
            }
        }

        internal void UnregisterBrush(D2DBrush brush)
        {
            _brushes.Remove(brush);
        }

        internal override void Clear(GraphicsPipe pipe, Color color)
        {
            base.Clear(pipe, color);
            _rt?.Clear(color.ToApi());
        }

        internal override void ApplyChanges(GraphicsPipe pipe)
        {
            base.ApplyChanges(pipe);
            OnRefreshed(pipe, this);
        }

        public void BeginDraw()
        {
            if (!_began && _currentBrush.IsValid)
            {
                _rt.BeginDraw();
                _began = true;
            }
        }

        public void EndDraw()
        {
            if (_began)
            {
                _rt.EndDraw();
                _began = false;
            }
        }

        public void FilledRectangle(RectangleF rectangle)
        {
            if (_began)
                _rt.FillRectangle(rectangle.ToApi(), _currentBrush.UnderlyingBrush);
        }

        public void FilledCircle(Vector2 pos, float radiusX, float radiusY)
        {
            Ellipse data = new Ellipse(){
                Point = pos.ToApi(),
                RadiusX = radiusX,
                RadiusY = radiusY,
            };

            if (_began)
                _rt.FillEllipse(data, _currentBrush.UnderlyingBrush);
        }

        public void FilledCircle(ref Ellipse data)
        {
            if (_began)
                _rt.FillEllipse(data, _currentBrush.UnderlyingBrush);
        }

        public void FilledRoundedRectangle(ref RoundedRectangle rectangle)
        {
            if (_began)
                _rt.FillRoundedRectangle(ref rectangle, _currentBrush.UnderlyingBrush);
        }

        public void FilledRoundedRectangle(RoundedRectangle rectangle)
        {
            if (_began)
                _rt.FillRoundedRectangle(ref rectangle, _currentBrush.UnderlyingBrush);
        }


        public void DrawRectangle(RectangleF rectangle, int lineWidth = 1)
        {
            if(_began)
                _rt.DrawRectangle(rectangle.ToApi(), _currentBrush.UnderlyingBrush, lineWidth);
        }

        public void DrawRectangle(RectangleF rectangle, StrokeStyle style,  float[] dashPattern = null, int lineWidth = 1 )
        {
            if (_began)
            {
#if DEBUG
                if (style.DashStyle == DashStyle.Custom && dashPattern == null)
                    throw new InvalidOperationException("No dash pattern array provided while using 'custom' dash style");
#endif

                _rt.DrawRectangle(rectangle.ToApi(), _currentBrush.UnderlyingBrush, lineWidth, style);
            }
        }

        public void DrawRoundedRectangle(RoundedRectangle rect, int lineWidth = 1)
        {
            if (_began)
                _rt.DrawRoundedRectangle(rect, _currentBrush.UnderlyingBrush, lineWidth);
        }

        public void DrawRoundedRectangle(RoundedRectangle rect, StrokeStyle style, float[] dashPattern = null, int lineWidth = 1)
        {
            if (_began)
            {
#if DEBUG
                if (style.DashStyle == DashStyle.Custom && dashPattern == null)
                    throw new InvalidOperationException("No dash pattern array provided while using 'custom' dash style");
#endif

                _rt.DrawRoundedRectangle(rect, _currentBrush.UnderlyingBrush, lineWidth, style);
            }
        }

        public void DrawCircle(Vector2 center, float radiusX, float radiusY, int lineWidth = 1)
        {
            if (_began)
            {
                Ellipse data = new Ellipse(center.ToApi(), radiusX, radiusY);
                _rt.DrawEllipse(data, _currentBrush.UnderlyingBrush, lineWidth);
            }
        }

        public void DrawCircle(ref Ellipse data, int lineWidth = 1)
        {
            if (_began)
                _rt.DrawEllipse(data, _currentBrush.UnderlyingBrush, lineWidth);
        }

        public void DrawCircle(ref Ellipse data, StrokeStyle style, int lineWidth = 1)
        {
            if (_began)
            {
#if DEBUG
                if(style.DashStyle == DashStyle.Custom)
                    throw new NotSupportedException("DashStyle.Custom is not supported for circle drawing.");
#endif

                _rt.DrawEllipse(data, _currentBrush.UnderlyingBrush, lineWidth, style);
            }
        }

        public void DrawLine(Vector2 point1, Vector2 point2, Color color, int lineWidth = 1)
        {
            if (_began)
                _rt.DrawLine(point1.ToApi(), point2.ToApi(), _currentBrush.UnderlyingBrush, lineWidth);
        }

        public void DrawLine(Vector2 point1, Vector2 point2, Color color, StrokeStyle style, int lineWidth = 1)
        {
            if (_began)
            {
#if DEBUG
                if (style.DashStyle == DashStyle.Custom)
                    throw new NotSupportedException("DashStyle.Custom is not supported for line drawing.");
#endif

                _rt.DrawLine(point1.ToApi(), point2.ToApi(), _currentBrush.UnderlyingBrush, lineWidth, style);
            }
        }


        public void DrawText(string text, TextFormat format, RectangleF bounds, DrawTextOptions options = DrawTextOptions.None, MeasuringMode measuring = MeasuringMode.Natural)
        {
            if(_began)
                _rt.DrawText(text, format, bounds.ToApi(), _currentBrush.UnderlyingBrush, options, measuring);
        }

        /// <summary>Pushes a clipping rectangle onto the surface. Anything outside of its bounds will not be rendered.</summary>
        /// <param name="rect">The clipping rectangle bounds.</param>
        /// <param name="antiAliasing">The anti-aliasing mode to use.</param>
        public void PushClip(ref RectangleF rect, AntialiasMode antiAliasing = AntialiasMode.Aliased)
        {
            _rt.PushAxisAlignedClip(rect.ToApi(), antiAliasing);
        }

        /// <summary>Removes the clipping rectangle that was previously pushed on to the surface.</summary>
        public void PopClip()
        {
            _rt.PopAxisAlignedClip();
        }

        private void BuildD2DSurface(SharpDX.Direct3D11.Resource res)
        {
            _surface = res.QueryInterface<Surface>();

            if (_surface != null)
            {
                _texture = TextureResource;
                _desc = _surface.Description;

                RenderTargetProperties rtp = new RenderTargetProperties(new PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
                _rt = new D2DRenderTarget(_d2dFactory, _surface, rtp);
                _rt.TextAntialiasMode = _textAntiAlias;
                _rt.TextRenderingParams = new RenderingParams(_dWriteFactory, 128, 0, 1, PixelGeometry.Rgb, RenderingMode.CleartypeNatural);

                //refresh all brushes
                for (int i = 0; i < _brushes.Count; i++)
                    _brushes[i].RefreshBrush();
            }
        }

        private void ReleaseD2DSurface()
        {
            _surface?.Dispose();
            _rt?.Dispose();
            _began = false;
        }

        protected override void OnDispose()
        {
            //dispose of all registered brushes.
            for (int i = 0; i < _brushes.Count; i++)
                _brushes[i].Dispose();

            ReleaseD2DSurface();
            _brushes.Clear();
            _texture = null;
        }

        /// <summary>Gets the underlying Direct2D render target instance.</summary>
        public D2DRenderTarget UnderlyingTarget { get { return _rt; } }

        /// <summary>Gets or sets the current drawing brush. Setting it to null will revert the surface to its default brush.</summary>
        public D2DBrush CurrentBrush
        {
            get { return _currentBrush; }
            set
            {
                if (value != null)
                    _currentBrush = value;
                else
                    _currentBrush = _defaultBrush;
            }
        }

        /// <summary>Gets whether or not the surface is ready to be drawn to. 
        /// Usually this means the render target is ready (or not).</summary>
        public bool IsValid { get { return _rt != null; } }

        public TextAntialiasMode TextAntiAliasMode
        {
            get { return _textAntiAlias; }
            set
            {
                _textAntiAlias = value;

                if (_rt != null)
                    _rt.TextAntialiasMode = _textAntiAlias;
            }
        }

        internal override ShaderResourceView SRV { get; set; }

        internal override UnorderedAccessView UAV { get; set; }
    }
}
