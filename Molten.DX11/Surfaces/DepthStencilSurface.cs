using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A special kind of render surface for use as a depth-stencil buffer.</summary>
    public class DepthStencilSurface : Texture2DDX11, IDepthStencilSurface
    {
        DepthStencilView _depthView;
        DepthStencilView _readOnlyView;
        DepthStencilViewDescription _depthDesc;
        DepthFormat _depthFormat;
        Viewport _vp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="swapChain"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="depthBuffer">If true, a depth buffer will be created.</param>
        /// <param name="flags">Texture flags</param>
        internal DepthStencilSurface(RendererDX11 renderer,
            int width, 
            int height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            int mipCount = 1, 
            int arraySize = 1, 
            int sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, SharpDX.DXGI.Format.R24G8_Typeless, mipCount, arraySize, flags)
        {
            _depthFormat = format;
            _description.ArraySize = arraySize;
            _description.Format = GetFormat().ToApi();
            _depthDesc = new DepthStencilViewDescription();
            _depthDesc.Format = GetDSVFormat().ToApi();

            if (_sampleCount > 1)
            {
                _depthDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                _depthDesc.Flags = DepthStencilViewFlags.None;
                _depthDesc.Texture2DMSArray = new DepthStencilViewDescription.Texture2DMultisampledArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _depthDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                _depthDesc.Flags = DepthStencilViewFlags.None;
                _depthDesc.Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                    MipSlice = 0,
                };
            }

            UpdateViewport();
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDescription desc)
        {
            base.SetSRVDescription(ref desc);

            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    desc.Format = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
                    break;

                case DepthFormat.R32_Typeless:
                    desc.Format = SharpDX.DXGI.Format.R32_Float;
                    break;
            }
        }

        private void UpdateViewport()
        {
            _vp = new Viewport(0, 0, _description.Width, _description.Height);
        }

        private GraphicsFormat GetFormat()
        {
            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return GraphicsFormat.R24G8_Typeless;
                case DepthFormat.R32_Typeless:
                    return GraphicsFormat.R32_Typeless;
            }
        }

        private GraphicsFormat GetDSVFormat()
        {
            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return GraphicsFormat.D24_UNorm_S8_UInt;
                case DepthFormat.R32_Typeless:
                    return GraphicsFormat.D32_Float;
            }
        }

        private DepthStencilViewFlags GetReadOnlyFlags()
        {
            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil;
                case DepthFormat.R32_Typeless:
                    return DepthStencilViewFlags.ReadOnlyDepth;
            }
        }

        protected override SharpDX.Direct3D11.Resource CreateResource(bool resize)
        {
            _depthView?.Dispose();
            _description.Width = Math.Max(1, _description.Width);
            _description.Height = Math.Max(1, _description.Height);

            // Create render target texture
            _texture = base.CreateResource(resize) as Texture2D;

            _depthDesc.Flags = DepthStencilViewFlags.None;
            _depthView = new DepthStencilView(Device.D3d, _texture, _depthDesc);

            // Create read-only depth view for passing to shaders.
            _depthDesc.Flags = GetReadOnlyFlags();
            _readOnlyView = new DepthStencilView(Device.D3d, _texture, _depthDesc);
            _depthDesc.Flags = DepthStencilViewFlags.None;

            return _texture;
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
            UpdateViewport();
        }

        internal void Clear(PipeDX11 pipe, DepthClearFlags clearFlags = DepthClearFlags.Depth, float depth = 1.0f, byte stencil = 0)
        {
            if (_depthView == null)
                CreateTexture(false);

            pipe.Context->ClearDepthStencilView(_depthView, clearFlags, depth, stencil);
        }

        public void Clear(DepthClearFlags clearFlags = DepthClearFlags.Depth, float depth = 1.0f, byte stencil = 0)
        {
            Clear(Device, clearFlags, depth, stencil);
        }

        private protected override void OnPipelineDispose()
        {
            DisposeObject(ref _depthView);
            DisposeObject(ref _readOnlyView);

            base.OnPipelineDispose();
        }

        /// <summary>Gets the DepthStencilView instance associated with this surface.</summary>
        internal DepthStencilView DepthView => _depthView;

        /// <summary>Gets the read-only DepthStencilView instance associated with this surface.</summary>
        internal DepthStencilView ReadOnlyView => _readOnlyView;

        /// <summary>Gets the depth-specific format of the surface.</summary>
        public DepthFormat DepthFormat => _depthFormat;

        /// <summary>Gets the viewport of the <see cref="DepthStencilSurface"/>.</summary>
        public Viewport Viewport => _vp;
    }
}
