using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A special kind of render surface for use as a depth-stencil buffer.</summary>
    public unsafe class DepthStencilSurface : Texture2D, IDepthStencilSurface
    {
        ID3D11DepthStencilView* _depthView;
        ID3D11DepthStencilView* _readOnlyView;
        DepthStencilViewDesc _depthDesc;
        DepthFormat _depthFormat;
        Viewport _vp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="mipCount"></param>
        /// <param name="arraySize"></param>
        /// <param name="aaLevel"></param>
        /// <param name="msaa"></param>
        /// <param name="flags">Texture flags</param>
        internal DepthStencilSurface(RenderService renderer,
            uint width, 
            uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1, 
            uint arraySize = 1, 
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality msaa = MSAAQuality.Default,
            TextureFlags flags = TextureFlags.None, 
            string name = "surface")
            : base(renderer, width, height, Format.FormatR24G8Typeless, mipCount, arraySize, flags, aaLevel, msaa)
        {
            _depthFormat = format;
            _desc.ArraySize = arraySize;
            _desc.Format = GetFormat().ToApi();
            _depthDesc = new DepthStencilViewDesc();
            _depthDesc.Format = GetDSVFormat().ToApi();

            name = name ?? "surface";
            Name = $"depth_{name}";

            if (MultiSampleLevel >= AntiAliasLevel.X2)
            {
                _depthDesc.ViewDimension = DsvDimension.Texture2Dmsarray;
                _depthDesc.Flags = 0U; // DsvFlag.None;
                _depthDesc.Texture2DMSArray = new Tex2DmsArrayDsv()
                {
                    ArraySize = _desc.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _depthDesc.ViewDimension = DsvDimension.Texture2Darray;
                _depthDesc.Flags = 0U; //DsvFlag.None;
                _depthDesc.Texture2DArray = new Tex2DArrayDsv()
                {
                    ArraySize = _desc.ArraySize,
                    FirstArraySlice = 0,
                    MipSlice = 0,
                };
            }

            UpdateViewport();
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
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
            _vp = new Viewport(0, 0, (int)_desc.Width, (int)_desc.Height);
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
                    return DsvFlag.Depth | DsvFlag.Stencil;
                case DepthFormat.R32_Typeless:
                    return DsvFlag.Depth;
            }
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            _desc.Width = Math.Max(1, _desc.Width);
            _desc.Height = Math.Max(1, _desc.Height);

            // Create render target texture
            NativeTexture = (ID3D11Texture2D1*)base.CreateResource(resize);

            _depthDesc.Flags = 0; // DsvFlag.None;
            SubresourceData* subData = null;
            ID3D11Resource* res = (ID3D11Resource*)NativeTexture;

            DeviceDX11 device = Device as DeviceDX11;
            device.Ptr->CreateDepthStencilView(res, ref _depthDesc, ref _depthView);

            // Create read-only depth view for passing to shaders.
            _depthDesc.Flags = (uint)GetReadOnlyFlags();
            device.Ptr->CreateDepthStencilView(res, ref _depthDesc, ref _readOnlyView);
            _depthDesc.Flags = (uint)DsvFlag.None;

            return res;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
            UpdateViewport();
        }

        internal void Clear(CommandQueueDX11 cmd, DepthClearFlags clearFlags = DepthClearFlags.Depth, float depth = 1.0f, byte stencil = 0)
        {
            if (_depthView == null)
                CreateTexture(false);

            cmd.Native->ClearDepthStencilView(_depthView, (uint)clearFlags, depth, stencil);
        }

        public void Clear(DepthClearFlags flags, GraphicsPriority priority, float depth = 1.0f, byte stencil = 0)
        {
            if (priority == GraphicsPriority.Immediate)
            {
                Clear((Device as DeviceDX11).Cmd, flags, depth, stencil);
            }
            else
            {
                QueueChange(new DepthClearChange()
                {
                    Flags = flags,
                    Surface = this,
                });
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            base.GraphicsRelease();
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
