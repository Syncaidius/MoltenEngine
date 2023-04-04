using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe class DeviceDX12 : DeviceDXGI
    {
        ID3D12Device10* _native;
        IDXGIAdapter4* _adapter;
        DeviceBuilderDX12 _builder;
        CommandQueueDX12 _cmdDirect;

        public DeviceDX12(RenderService renderer, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter, DeviceBuilderDX12 deviceBuilder) : 
            base(renderer, manager, adapter)
        {
            _builder = deviceBuilder;
        }

        internal void Initialize()
        {
            HResult r = _builder.CreateDevice(this, out PtrRef);
            if (!_builder.CheckResult(r, () => $"Failed to initialize {nameof(DeviceDX12)}"))
                return;

            CommandQueueDesc cmdDesc = new CommandQueueDesc()
            {
                Type = CommandListType.Direct,
            };

            _cmdDirect = new CommandQueueDX12(Log, this, _builder, ref cmdDesc);
        }

        protected override void OnDispose()
        {
            _cmdDirect.Dispose();
            base.OnDispose();
        }

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            throw new NotImplementedException();
        }

        protected override HlslPass OnCreateShaderPass(HlslShader shader, string name)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags flags, uint numVertices, T[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, ushort[] initialData)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, uint[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint numElements, T[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateStagingBuffer(bool canRead, bool canWrite, uint byteCapacity)
        {
            throw new NotImplementedException();
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        public override INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(ITexture source, ITexture destination)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(ITexture source, ITexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
        {
            throw new NotImplementedException();
        }

        public override IRenderSurface2D CreateSurface(uint width, uint height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8_Typeless, GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The underlying, native device pointer.
        /// </summary>
        internal ID3D12Device10* Ptr => _native;

        /// <summary>
        /// Gets a protected reference to the underlying device pointer.
        /// </summary>
        protected ref ID3D12Device10* PtrRef => ref _native;

        public override CommandQueueDX12 Cmd => _cmdDirect;
    }
}
