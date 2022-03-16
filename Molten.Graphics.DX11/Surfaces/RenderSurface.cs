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
        ViewportF _vp;
        Rectangle _vpBounds;

        internal RenderSurface(
            RendererDX11 renderer,
            uint width,
            uint height,
            Format format = Format.FormatR8G8B8A8SNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            uint sampleCount = 1,
            TextureFlags flags = TextureFlags.None, string name = null)
            : base(renderer, width, height, format, mipCount, arraySize, flags, sampleCount)
        {
            Viewport = new ViewportF(0, 0, width, height);

            Name = $"Surface_{name ?? GetType().Name}";
            RTV = new RenderTargetView(renderer.Device)
            {
                Desc = new RenderTargetViewDesc()
                {
                    Format = DxgiFormat,
                }
            };
        }

        internal virtual void Clear(DeviceContext pipe, Color color)
        {
            Apply(pipe);

            if (RTV.Ptr != null)
            {
                Color4 c4 = color;
                pipe.Native->ClearRenderTargetView(RTV.Ptr, (float*)&c4);
            }
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            RTV.Release();

            ID3D11Resource* resource =  base.CreateResource(resize);
            SetRTVDescription(ref RTV.Desc);

            if (_description.SampleDesc.Count > 1)
            {
                RTV.Desc.ViewDimension = RtvDimension.RtvDimensionTexture2Dmsarray;
                RTV.Desc.Texture2DMSArray = new Tex2DmsArrayRtv
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                RTV.Desc.ViewDimension = RtvDimension.RtvDimensionTexture2Darray;
                RTV.Desc.Texture2DArray = new Tex2DArrayRtv()
                {
                    ArraySize = _description.ArraySize,
                    MipSlice = 0,
                    FirstArraySlice = 0,
                };
            }

            RTV.Create(resource);
            return resource;
        }

        protected virtual void SetRTVDescription(ref RenderTargetViewDesc desc) { }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _description.Width = (uint)newWidth;
            _description.Height = (uint)newHeight;
            _description.Format = newFormat;
            //_description.MipLevels = newMipMapCount; // NOTE: Do we set this on render targets?

            Viewport = new ViewportF(_vp.X, _vp.Y, newWidth, newHeight);
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
        internal override void PipelineRelease()
        {
            RTV.Release();
            base.PipelineRelease();
        }

        /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
        public ViewportF Viewport
        {
            get => _vp;
            protected set => _vp = value;
        }

        /// <summary>
        /// Gets the DX11 render target view (RTV) for the current render surface.
        /// </summary>
        internal RenderTargetView RTV { get; }

    }
}
