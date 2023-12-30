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
        CommandQueueDX12 _cmdDirect;
        ID3D12InfoQueue1* _debugInfo;
        uint _debugCookieID;

        public DeviceDX12(RenderService renderer, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter, DeviceBuilderDX12 deviceBuilder) :
            base(renderer, manager, adapter)
        {
            _builder = deviceBuilder;
            CapabilitiesDX12 = new CapabilitiesDX12();
        }

        protected override bool OnInitialize()
        {
            HResult r = _builder.CreateDevice(this, out PtrRef);
            if (!Renderer.Log.CheckResult(r, () => $"Failed to initialize {nameof(DeviceDX12)}"))
                return false;

            // Now we need to retrieve a debug info queue from the device.
            if (Settings.EnableDebugLayer)
            {
                void* ptr = null;
                Guid guidDebugInfo = ID3D12InfoQueue1.Guid;
                _native->QueryInterface(&guidDebugInfo, &ptr);
                _debugInfo = (ID3D12InfoQueue1*)ptr;
                _debugInfo->PushEmptyStorageFilter();

                uint debugCookieID = 0;
                r = _debugInfo->RegisterMessageCallback(new PfnMessageFunc(ProcessDebugMessage), MessageCallbackFlags.FlagNone, null, &debugCookieID);
                _debugCookieID = debugCookieID;

                if (!r.IsSuccess)
                    Log.Error("Failed to register debug callback");
            }

            CommandQueueDesc cmdDesc = new CommandQueueDesc()
            {
                Type = CommandListType.Direct,
            };

            _cmdDirect = new CommandQueueDX12(Log, this, _builder, ref cmdDesc);

            return true;
        }

        private void ProcessDebugMessage(MessageCategory category, MessageSeverity severity, MessageID id, byte* pDescription, void* prContext)
        {
            string desc = SilkMarshal.PtrToString((nint)pDescription, NativeStringEncoding.LPStr);
            string msg = $"[DX12] [Frame {Renderer.FrameID}] [{severity} - {category}] {desc}";

            switch (severity)
            {
                case MessageSeverity.Corruption:
                case MessageSeverity.Error:
                    Log.Error(msg);
                    break;

                case MessageSeverity.Warning:
                    Log.Warning(msg);
                    break;

                case MessageSeverity.Info:
                    Log.WriteLine(msg);
                    break;

                default:
                case MessageSeverity.Message:
                    Log.Write(msg);
                    break;
            }
        }

        protected override uint MinimumFrameBufferSize()
        {
            return 2;
        }

        protected override void OnDispose()
        {
            _cmdDirect.Dispose();

            if (_debugInfo != null)
            {
                _debugInfo->UnregisterMessageCallback(_debugCookieID);
                SilkUtil.ReleasePtr(ref _debugInfo);
            }
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

        protected override INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height,
            GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm, uint mipCount = 1)
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

        public override CommandQueueDX12 Queue => _cmdDirect;

        /// <summary>
        /// Gets DirectX 12-specific capabilities.
        /// </summary>
        public CapabilitiesDX12 CapabilitiesDX12 { get; }
    }
}
