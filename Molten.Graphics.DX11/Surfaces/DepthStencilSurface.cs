using Silk.NET.DXGI;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A special kind of render surface for use as a depth-stencil buffer.</summary>
    public unsafe class DepthStencilSurface : Texture2DDX11, IDepthStencilSurface
    {
        ID3D11DepthStencilView* _depthView;
        ID3D11DepthStencilView* _readOnlyView;
        DepthStencilViewDesc _depthDesc;
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
            uint width, 
            uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1, 
            uint arraySize = 1, 
            uint sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, Format.FormatR24G8Typeless, mipCount, arraySize, flags)
        {
            _depthFormat = format;
            _description.ArraySize = (uint)arraySize;
            _description.Format = GetFormat().ToApi();
            _depthDesc = new DepthStencilViewDesc();
            _depthDesc.Format = GetDSVFormat().ToApi();

            if (SampleCount > 1)
            {
                _depthDesc.ViewDimension = DsvDimension.DsvDimensionTexture2Dmsarray;
                _depthDesc.Flags = 0U; // DsvFlag.None;
                _depthDesc.Texture2DMSArray = new Tex2DmsArrayDsv()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _depthDesc.ViewDimension = DsvDimension.DsvDimensionTexture2Darray;
                _depthDesc.Flags = 0U; //DsvFlag.None;
                _depthDesc.Texture2DArray = new Tex2DArrayDsv()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                    MipSlice = 0,
                };
            }

            UpdateViewport();
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
        {
            base.SetSRVDescription(ref desc);

            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    desc.Format = Format.FormatR24UnormX8Typeless;
                    break;

                case DepthFormat.R32_Typeless:
                    desc.Format = Format.FormatR32Float;
                    break;
            }
        }

        private void UpdateViewport()
        {
            _vp = new Viewport(0, 0, (int)_description.Width, (int)_description.Height);
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

        private DsvFlag GetReadOnlyFlags()
        {
            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return DsvFlag.DsvReadOnlyDepth | DsvFlag.DsvReadOnlyStencil;
                case DepthFormat.R32_Typeless:
                    return DsvFlag.DsvReadOnlyDepth;
            }
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            _description.Width = Math.Max(1, _description.Width);
            _description.Height = Math.Max(1, _description.Height);

            // Create render target texture
            NativeTexture = (ID3D11Texture2D*)base.CreateResource(resize);

            _depthDesc.Flags = 0; // DsvFlag.None;
            SubresourceData* subData = null;
            ID3D11Resource* res = (ID3D11Resource*)NativeTexture;

            Device.NativeDevice->CreateDepthStencilView(res, ref _depthDesc, ref _depthView);

            // Create read-only depth view for passing to shaders.
            _depthDesc.Flags = (uint)GetReadOnlyFlags();
            Device.NativeDevice->CreateDepthStencilView(res, ref _depthDesc, ref _readOnlyView);
            _depthDesc.Flags = 0U; // DsvFlag.None;

            return res;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
            UpdateViewport();
        }

        internal void Clear(DeviceContext pipe, ClearFlag clearFlags = ClearFlag.ClearDepth, float depth = 1.0f, byte stencil = 0)
        {
            if (_depthView == null)
                CreateTexture(false);

            pipe.Native->ClearDepthStencilView(_depthView, (uint)clearFlags, depth, stencil);
        }

        public void Clear(DepthClearFlags flags, float depth = 1.0f, byte stencil = 0)
        {
            Clear(Device, (ClearFlag)flags, depth, stencil);
        }

        internal override void PipelineDispose()
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            base.PipelineDispose();
        }

        /// <summary>Gets the DepthStencilView instance associated with this surface.</summary>
        internal ID3D11DepthStencilView* DepthView => _depthView;

        /// <summary>Gets the read-only DepthStencilView instance associated with this surface.</summary>
        internal ID3D11DepthStencilView* ReadOnlyView => _readOnlyView;

        /// <summary>Gets the depth-specific format of the surface.</summary>
        public DepthFormat DepthFormat => _depthFormat;

        /// <summary>Gets the viewport of the <see cref="DepthStencilSurface"/>.</summary>
        public Viewport Viewport => _vp;
    }
}
