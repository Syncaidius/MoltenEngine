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
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public class RenderSurface : RenderSurfaceBase, IRenderSurface
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="swapChain"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="depthBuffer">If true, a depth buffer will be created.</param>
        internal RenderSurface(RendererDX11 renderer, 
            int width, 
            int height,
            Format format = SharpDX.DXGI.Format.R8G8B8A8_SNorm,
            int mipCount = 1,
            int arraySize = 1,
            int sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, format, mipCount, arraySize, sampleCount, flags)
        {
            _format = format;
        }

        protected override SharpDX.Direct3D11.Resource CreateTextureInternal(bool resize)
        {
            //create render target texture
            _texture = new Texture2D(Device.D3d, _description);
            RTV = new RenderTargetView(Device.D3d, TextureResource);
            return _texture;
        }
    }
}
