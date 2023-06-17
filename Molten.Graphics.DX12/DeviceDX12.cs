using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12
{
    internal unsafe class DeviceDX12 : DeviceDXGI
    {
        ID3D12Device10* _native;
        DeviceBuilderDX12 _builder;
        GraphicsQueueDX12 _cmdDirect;

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

            _cmdDirect = new GraphicsQueueDX12(Log, this, _builder, ref cmdDesc);
        }

        protected override void OnDispose()
        {
            _cmdDirect.Dispose();
            base.OnDispose();
        }

        protected override void OnBeginFrame(ThreadedList<ISwapChainSurface> surfaces)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndFrame(ThreadedList<ISwapChainSurface> surfaces)
        {
            throw new NotImplementedException();
        }

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            throw new NotImplementedException();
        }

        protected override HlslPass OnCreateShaderPass(HlslShader shader, string name)
        {
            throw new NotImplementedException();
        }

        protected override GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format, uint numElements, T[] initialData)
        {
            throw new NotImplementedException();
        }

        protected override INativeSurface OnCreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        protected override INativeSurface OnCreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
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

        public override ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize, GraphicsFormat format, GraphicsResourceFlags flags, AntiAliasLevel aaLevel = AntiAliasLevel.None, MSAAQuality aaQuality = MSAAQuality.Default, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GraphicsFormat format, uint cubeCount = 1, uint arraySize = 1, GraphicsResourceFlags flags = GraphicsResourceFlags.None, bool allowMipMapGen = false, string name = null)
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

        public override GraphicsQueueDX12 Queue => _cmdDirect;
    }
}
