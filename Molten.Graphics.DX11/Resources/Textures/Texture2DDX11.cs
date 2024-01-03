using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public unsafe class Texture2DDX11 : TextureDX11, ITexture2D
    {
        protected Texture2DDesc1 Desc;

        internal Texture2DDX11(
            DeviceDX11 device,
            uint width,
            uint height,
            GraphicsResourceFlags flags,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality msaa = MSAAQuality.Default,
            string name = null)
            : base(device, GraphicsTextureType.Texture2D, new TextureDimensions(width, height, 1, mipCount, arraySize), aaLevel, msaa, format, flags, name)
        {
            Desc = new Texture2DDesc1()
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(height, 1),
                MipLevels = mipCount,
                ArraySize = Math.Max(arraySize, 1),
                Format = format.ToApi(),
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)Flags.ToCpuFlags(),
                SampleDesc = new SampleDesc((uint)aaLevel, (uint)msaa),
                Usage = Flags.ToUsageFlags(),
                MiscFlags = (uint)Flags.ToMiscFlags(),
                TextureLayout = TextureLayout.LayoutUndefined,
            };
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex)
        {
            SubresourceData* subData = GetImmutableData(Desc.Usage);

            ID3D11Texture2D1* ptrTex = null;
            fixed (Texture2DDesc1* pDesc = &Desc)
                Device.Ptr->CreateTexture2D1(pDesc, subData, ref ptrTex);

            EngineUtil.Free(ref subData);
            handle.NativePtr = (ID3D11Resource*)ptrTex;
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            if (Desc.SampleDesc.Count > 1)
            {
                desc.ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2Dmsarray;
                desc.Texture2DMSArray = new Tex2DmsArraySrv()
                {
                    ArraySize = Desc.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2Darray;
                desc.Texture2DArray = new Tex2DArraySrv1()
                {
                    ArraySize = Desc.ArraySize,
                    MipLevels = Desc.MipLevels,
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
                ArraySize = Desc.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
                PlaneSlice = 0
            };

            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = Desc.Width * Desc.Height * Desc.ArraySize,
            };
        }

        protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
        {
            Desc.Width = dimensions.Width;
            Desc.Height = dimensions.Height;
            Desc.ArraySize = dimensions.ArraySize;
            Desc.MipLevels = dimensions.MipMapCount;
            Desc.Format = newFormat.ToApi();
            Desc.TextureLayout = TextureLayout.LayoutUndefined;
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount = 0,
            uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            Resize(priority, newWidth, newHeight, newArraySize, newMipMapCount, Depth, newFormat);
        }
    }
}
