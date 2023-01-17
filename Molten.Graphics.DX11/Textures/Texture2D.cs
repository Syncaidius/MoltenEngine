using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class Texture2D : TextureBase, ITexture2D
    {
        internal ID3D11Texture2D1* NativeTexture;
        protected Texture2DDesc1 _description;

        /// <summary>Creates a new instance of <see cref="Texture2D"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        /// <param name="flags">A set of flags to override those of the provided texture.</param>
        internal Texture2D(Texture2D descTexture, TextureFlags flags)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, 
                  descTexture.DxgiFormat, descTexture.MipMapCount, descTexture.ArraySize, flags)
        { }

        /// <summary>Creates a new instance of <see cref="Texture2D"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal Texture2D(Texture2D descTexture)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, descTexture.DxgiFormat, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags)
        { }

        internal Texture2D(
            RendererDX11 renderer,
            uint width,
            uint height,
            Format format = Format.FormatR8G8B8A8Unorm,
            uint mipCount = 1,
            uint arraySize = 1,
            TextureFlags flags = TextureFlags.None,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality msaa = MSAAQuality.Default)
            : base(renderer, width, height, 1, mipCount, arraySize, aaLevel, msaa, format, flags)
        {
            _description = new Texture2DDesc1()
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(height, 1),
                MipLevels = mipCount,
                ArraySize = Math.Max(arraySize, 1),
                Format = format,
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)GetAccessFlags(),
                SampleDesc = new SampleDesc()
                {
                    Count = (uint)aaLevel,
                    Quality = (uint)msaa,
                },
                Usage = GetUsageFlags(),
                MiscFlags = (uint)GetResourceFlags(),
                TextureLayout = TextureLayout.None
            };
        }

        public Texture2DProperties Get2DProperties()
        {
            return new Texture2DProperties()
            {
                Width = Width,
                Height = Height,
                ArraySize = ArraySize,
                Flags = Flags,
                Format = DataFormat,
                MipMapLevels = MipMapCount,
                MultiSampleLevel = MultiSampleLevel,
            };
        }

        protected override unsafe ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            NativeDevice.Ptr->CreateTexture2D1(ref _description, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            if (_description.SampleDesc.Count > 1)
            {
                desc.ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2Dmsarray;
                desc.Texture2DMSArray = new Tex2DmsArraySrv()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2Darray;
                desc.Texture2DArray = new Tex2DArraySrv1()
                {
                    ArraySize = _description.ArraySize,
                    MipLevels = _description.MipLevels,
                    MostDetailedMip = 0,
                    FirstArraySlice = 0,
                    PlaneSlice = 0,
                };
            }
        }

        protected override void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc)
        {
            desc.Format = srvDesc.Format;
            desc.ViewDimension = UavDimension.Texture2Darray;
            
            desc.Texture2DArray = new Tex2DArrayUav1()
            {
                ArraySize = _description.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
                PlaneSlice = 0
            };

            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = _description.Width * _description.Height * _description.ArraySize,
            };
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, 
            uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _description.ArraySize = newArraySize;
            _description.Width = newWidth;
            _description.Height = newHeight;
            _description.MipLevels = newMipMapCount;
            _description.Format = newFormat;
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount = 0, uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount == 0 ? MipMapCount : newMipMapCount,
                NewArraySize = newArraySize == 0 ? _description.ArraySize : newArraySize,
                NewFormat = newFormat == GraphicsFormat.Unknown ? DxgiFormat : newFormat.ToApi()
            });
        }
    }
}
