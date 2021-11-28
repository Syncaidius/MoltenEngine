using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class RenderSurface : Texture2DDX11, IRenderSurface
    {
        /// <summary>The viewport which represents the current render surface.</summary>
        internal Viewport VP;

        /// <summary>The underlying render-target-view (RTV).</summary>
        protected RenderTargetView _rtv;
        RenderTargetViewDescription _rtvDesc;

        internal RenderSurface(
            RendererDX11 renderer,
            int width,
            int height,
            Format format = Format.FormatR8G8B8A8SNorm,
            int mipCount = 1,
            int arraySize = 1,
            int sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, format, mipCount, arraySize, flags, sampleCount)
        {
            VP = new Viewport(0, 0, width, height);
            _rtvDesc = new RenderTargetViewDescription();
            _rtvDesc.Format = _format;
        }

        internal virtual void Clear(PipeDX11 pipe, Color color)
        {
            Apply(pipe);

            if(RTV != null)
                pipe.Context.ClearRenderTargetView(RTV, color.ToApi());
        }

        protected override SharpDX.Direct3D11.Resource CreateResource(bool resize)
        {
            SharpDX.Direct3D11.Resource resource =  base.CreateResource(resize);
            _rtv?.Dispose();

            SetRTVDescription(ref _rtvDesc);
            if (_description.SampleDescription.Count > 1)
            {
                _rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                _rtvDesc.Texture2DMSArray = new RenderTargetViewDescription.Texture2DMultisampledArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _rtvDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                _rtvDesc.Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    MipSlice = 0,
                    FirstArraySlice = 0,
                };
            }

            _rtv = new RenderTargetView(Device.D3d, resource, _rtvDesc);
            return resource;
        }

        protected virtual void SetRTVDescription(ref RenderTargetViewDescription desc) { }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            _description.Width = newWidth;
            _description.Height = newHeight;
            _description.Format = newFormat;
            //_description.MipLevels = newMipMapCount; // NOTE: Do we set this on render targets?

            VP.Width = newWidth;
            VP.Height = newHeight;
        }

        public void Clear(Color color)
        {
            QueueChange(new SurfaceClearChange()
            {
                Color = color,
                Surface = this,
            });
        }

        /// <summary>Called when the render target needs to be disposed.</summary>
        private protected override void OnPipelineDispose()
        {
            DisposeObject(ref _rtv);
            base.OnPipelineDispose();
        }

        /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
        public Viewport Viewport => VP;

        /// <summary>
        /// Gets the DX11 render target view (RTV) for the current render surface.
        /// </summary>
        internal RenderTargetView RTV => _rtv;
    }
}
