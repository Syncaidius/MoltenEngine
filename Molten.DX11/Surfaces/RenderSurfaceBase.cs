using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class RenderSurfaceBase : TextureAsset2D, IRenderSurface
    {
        /// <summary>The viewport which represents the current render surface.</summary>
        internal Viewport VP;

        /// <summary>The underlying render-target-view (RTV).</summary>
        internal RenderTargetView RTV;

        internal RenderSurfaceBase(
            GraphicsDeviceDX11 device, 
            int width, 
            int height, 
            Format format, 
            int mipCount, 
            int arraySize,
            int sampleCount, 
            TextureFlags flags)
            : base(device, width, height, format, mipCount, arraySize, flags, sampleCount)
        {
            VP = new Viewport(0, 0, width, height);
        }

        internal virtual void Clear(GraphicsPipe pipe, Color color)
        {
            ApplyChanges(pipe);

            if(RTV != null)
                pipe.Context.ClearRenderTargetView(RTV, color.ToApi());
        }

        protected override void OnSetSize(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize)
        {
            _description.Width = newWidth;
            _description.Height = newHeight;
            //_description.MipLevels = newMipMapCount; // NOTE: Do we set this on render targets?
            VP = new Viewport(0, 0, newWidth, newHeight);
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
            DisposeObject(ref RTV);
            base.OnPipelineDispose();
        }

        /// <summary>Gets the viewport that defines the renderable area of the render target.</summary>
        public Viewport Viewport
        {
            get { return VP; }
        }
    }
}
