using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class RenderSurface : Texture2DDX11, IRenderSurface
    {
        /// <summary>The viewport which represents the current render surface.</summary>
        internal Viewport VP;

        RenderTargetViewDesc _rtvDesc;

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
            _rtvDesc = new RenderTargetViewDesc();
            _rtvDesc.Format = DxgiFormat;
        }

        internal virtual void Clear(PipeDX11 pipe, Color color)
        {
            Apply(pipe);

            if (RTV != null)
            {
                Color4 c4 = color;
                pipe.Context->ClearRenderTargetView(RTV, (float*)&c4);
            }

        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            ID3D11Resource* resource =  base.CreateResource(resize);
            SilkUtil.ReleasePtr(ref RTV);

            SetRTVDescription(ref _rtvDesc);
            if (_description.SampleDescription.Count > 1)
            {
                _rtvDesc.ViewDimension = RtvDimension.RtvDimensionTexture2Dmsarray;
                _rtvDesc.Texture2DMSArray = new Tex2DmsArrayRtv
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _rtvDesc.ViewDimension = RtvDimension.RtvDimensionTexture2Darray;
                _rtvDesc.Texture2DArray = new Tex2DArrayRtv()
                {
                    ArraySize = _description.ArraySize,
                    MipSlice = 0,
                    FirstArraySlice = 0,
                };
            }

            Device.Native->CreateRenderTargetView(NativePtr, ref _rtvDesc, ref RTV);
            return resource;
        }

        protected virtual void SetRTVDescription(ref RenderTargetViewDesc desc) { }

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
        internal override void PipelineDispose()
        {
            SilkUtil.ReleasePtr(ref RTV);
            base.PipelineDispose();
        }

        /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
        public Viewport Viewport => VP;

        /// <summary>
        /// Gets the DX11 render target view (RTV) for the current render surface.
        /// </summary>
        protected internal ID3D11RenderTargetView* RTV;
    }
}
