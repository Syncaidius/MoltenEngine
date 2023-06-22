using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public unsafe class RenderSurface2DDX11 : Texture2DDX11, IRenderSurface2D
    {
        RTViewDX11[] _rtvs;

        internal RenderSurface2DDX11(
            GraphicsDevice device,
            uint width,
            uint height,
            GraphicsResourceFlags flags = GraphicsResourceFlags.None,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality msaa = MSAAQuality.Default,
            bool allowMipMapGen = false, 
            string name = null)
            : base(device, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, allowMipMapGen, name)
        {
            Viewport = new ViewportF(0, 0, width, height);

            Name = $"Surface_{name ?? GetType().Name}";
        }

        protected override ResourceHandleDX11<ID3D11Resource> CreateHandle()
        {
            return new RenderSurfaceHandleDX11(this);
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex)
        {
            base.CreateTexture(device, handle, handleIndex);

            RenderSurfaceHandleDX11 rsHandle = handle as RenderSurfaceHandleDX11;
            ref RenderTargetViewDesc1 desc = ref rsHandle.RTV.Desc;
            desc.Format = DxgiFormat;

            SetRTVDescription(ref desc);

            if (Desc.SampleDesc.Count > 1)
            {
                desc.ViewDimension = RtvDimension.Texture2Dmsarray;
                desc.Texture2DMSArray = new Tex2DmsArrayRtv
                {
                    ArraySize = Desc.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                desc.ViewDimension = RtvDimension.Texture2Darray;
                desc.Texture2DArray = new Tex2DArrayRtv1()
                {
                    ArraySize = Desc.ArraySize,
                    MipSlice = 0,
                    FirstArraySlice = 0,
                    PlaneSlice = 0,
                };
            }

            rsHandle.RTV.Create();
        }

        protected virtual void SetRTVDescription(ref RenderTargetViewDesc1 desc) { }

        protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
        {
            base.UpdateDescription(dimensions, newFormat);

            Desc.MipLevels = 1; // NOTE: Do we set this on render targets?
            Viewport = new ViewportF(Viewport.X, Viewport.Y, dimensions.Width, dimensions.Height);
        }

        internal virtual void OnClear(GraphicsQueueDX11 cmd, Color color)
        {
            RenderSurfaceHandleDX11 rsHandle = Handle as RenderSurfaceHandleDX11;

            if (rsHandle.RTV.Ptr != null)
            {
                Color4 c4 = color;
                cmd.Ptr->ClearRenderTargetView(rsHandle.RTV, (float*)&c4);
            }
        }

        public void Clear(GraphicsPriority priority, Color color)
        {
            QueueTask(priority, new SurfaceClearTask()
            {
                Color = color,
                Surface = this,
            });
        }

        /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
        public ViewportF Viewport { get; protected set; }
    }
}
