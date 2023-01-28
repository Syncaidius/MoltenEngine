using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Molten.Graphics
{
    internal unsafe class DeviceBuilderDX12
    {
        RendererDX12 _renderer;
        D3D12 _api;

        readonly D3DFeatureLevel[] _featureLevels = new D3DFeatureLevel[]
        {
            D3DFeatureLevel.Level122,
            D3DFeatureLevel.Level121,
            D3DFeatureLevel.Level120,
            D3DFeatureLevel.Level111,
            D3DFeatureLevel.Level110
        };

        internal DeviceBuilderDX12(D3D12 api, RendererDX12 renderer)
        {
            _renderer = renderer;
            _api = api;
        }

        internal bool CheckResult(HResult r, Func<string> getMsg = null)
        {
            if (r.IsFailure)
            {
                if (getMsg != null)
                {
                    string msg = getMsg() + $" - Code: {r}";
                    _renderer.Log.Error(msg);
                }

                return false;
            }

            return true;
        }

        internal  HResult CreateDevice(
            DisplayAdapterDXGI adapter,
            out ID3D12Device10* device)
        {
            device = null;

            IUnknown* ptrAdapter = (IUnknown*)adapter.Native;
            void* ptrDevice = null;

            Guid guidDevice = ID3D12Device.Guid;
            HResult r = _api.CreateDevice(ptrAdapter, _featureLevels.Last(), &guidDevice, &ptrDevice);

            if (!r.IsFailure)
                device = (ID3D12Device10*)ptrDevice;

            return r;
        }

        internal void GetCapabilities(GraphicsSettings settings, DisplayAdapterDXGI adapter)
        {
            GraphicsCapabilities cap = adapter.Capabilities;
            ID3D12Device10* device = null;
            HResult r = CreateDevice(adapter, out device);
            if(!CheckResult(r, () => $"Failed to detect capabilities for adapter '{adapter.Name}'"))
                return;

            FeatureDataFeatureLevels dataFeatures = new FeatureDataFeatureLevels();
            fixed (D3DFeatureLevel* ptrLevels = _featureLevels)
            {
                dataFeatures.NumFeatureLevels = (uint)_featureLevels.Length;
                dataFeatures.PFeatureLevelsRequested = ptrLevels;
                GetFeatureSupport(device, Feature.FeatureLevels, &dataFeatures);
            }

            switch (dataFeatures.MaxSupportedFeatureLevel)
            {
                case D3DFeatureLevel.Level122:
                    cap.Api = GraphicsApi.DirectX12_2;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model6_0;
                    break;

                case D3DFeatureLevel.Level121:
                    cap.Api = GraphicsApi.DirectX12_1;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model6_0;
                    break;

                case D3DFeatureLevel.Level120:
                    cap.Api = GraphicsApi.DirectX12_0;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;

                case D3DFeatureLevel.Level111:
                    cap.Api = GraphicsApi.DirectX11_1;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;

                case D3DFeatureLevel.Level110:
                    cap.Api = GraphicsApi.DirectX11_0;
                    cap.UnorderedAccessBuffers.MaxSlots = 8;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;
            }

            // Check if a lower shader model is supported instead of API's model.
            FeatureDataShaderModel maxSM = new FeatureDataShaderModel(cap.MaxShaderModel.ToApi());
            GetFeatureSupport(device, Feature.ShaderModel, &maxSM);
            cap.MaxShaderModel = maxSM.HighestShaderModel.FromApi();

            FeatureDataD3D12Options features12_0 = GetFeatureSupport<FeatureDataD3D12Options>(device, Feature.D3D12Options);
            FeatureDataD3D12Options1 features12_1 = GetFeatureSupport<FeatureDataD3D12Options1>(device, Feature.D3D12Options1);
            FeatureDataD3D12Options2 features12_2 = GetFeatureSupport<FeatureDataD3D12Options2>(device, Feature.D3D12Options2);
            FeatureDataD3D12Options3 features12_3 = GetFeatureSupport<FeatureDataD3D12Options3>(device, Feature.D3D12Options3);
            FeatureDataD3D12Options4 features12_4 = GetFeatureSupport<FeatureDataD3D12Options4>(device, Feature.D3D12Options4);
            FeatureDataD3D12Options5 features12_5 = GetFeatureSupport<FeatureDataD3D12Options5>(device, Feature.D3D12Options5); // Raytracing starts here.
            FeatureDataD3D12Options6 features12_6 = GetFeatureSupport<FeatureDataD3D12Options6>(device, Feature.D3D12Options6);
            FeatureDataD3D12Options7 features12_7 = GetFeatureSupport<FeatureDataD3D12Options7>(device, Feature.D3D12Options7); // Variable shading rate starts here.
            FeatureDataD3D12Options8 features12_8 = GetFeatureSupport<FeatureDataD3D12Options8>(device, Feature.D3D12Options8);
            FeatureDataD3D12Options9 features12_9 = GetFeatureSupport<FeatureDataD3D12Options9>(device, Feature.D3D12Options9);
            FeatureDataD3D12Options10 features12_10 = GetFeatureSupport<FeatureDataD3D12Options10>(device, Feature.D3D12Options10);
            FeatureDataD3D12Options11 features12_11 = GetFeatureSupport<FeatureDataD3D12Options11>(device, Feature.D3D12Options11);
            FeatureDataD3D12Options12 features12_12 = GetFeatureSupport<FeatureDataD3D12Options12>(device, Feature.D3D12Options12);
            FeatureDataD3D12Options13 features12_13 = GetFeatureSupport<FeatureDataD3D12Options13>(device, Feature.D3D12Options13);

            cap.MaxTexture1DSize = 16384;
            cap.MaxTexture2DSize = 16384;
            cap.MaxTexture3DSize = 2048;
            cap.MaxTextureCubeSize = 16384;
            cap.MaxAnisotropy = 16;
            cap.BlendLogicOp = features12_0.OutputMergerLogicOp > 0;
            cap.MaxShaderSamplers = 16;
            cap.OcclusionQueries = true;
            cap.HardwareInstancing = true;
            cap.MaxTextureArraySlices = 2048;               // D3D11_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION (2048 array slices)
            cap.TextureCubeArrays = true;
            cap.NonPowerOfTwoTextures = true;
            cap.MaxAllocatedSamplers = 4096;                // D3D11_REQ_SAMPLER_OBJECT_COUNT_PER_DEVICE (4096) - Total number of sampler objects per context
            cap.MaxPrimitiveCount = uint.MaxValue;          // (2^32) – 1 = uint.maxValue (4,294,967,295)
            cap.RasterizerOrderViews = features12_0.ROVsSupported != 0;
            cap.ConservativeRasterization = (ConservativeRasterizationLevel)features12_0.ConservativeRasterizationTier;

            cap.VertexBuffers.MaxSlots = 32;                // D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT = 32;
            cap.VertexBuffers.MaxElementsPerVertex = 32;    // D3D11_STANDARD_VERTEX_ELEMENT_COUNT = 32;
            cap.VertexBuffers.MaxElements = uint.MaxValue;  // (2^32) – 1 = uint.maxValue (4,294,967,295)

            DetectShaderStages(device, cap, 
                features12_0.MinPrecisionSupport, 
                features12_0.DoublePrecisionFloatShaderOps != 0, 
                features12_1.Int64ShaderOps != 0);

            SilkUtil.ReleasePtr(ref device);
        }

        private void DetectShaderStages(ID3D12Device10* device, GraphicsCapabilities cap, 
            ShaderMinPrecisionSupport minPrecision,
            bool float64Support, bool int64Support)
        {
            bool bit10 = (minPrecision & ShaderMinPrecisionSupport.Support10Bit) == ShaderMinPrecisionSupport.Support10Bit;
            bool bit16 = (minPrecision & ShaderMinPrecisionSupport.Support16Bit) == ShaderMinPrecisionSupport.Support16Bit;

            cap.SetShaderCap(nameof(ShaderStageCapabilities.Float10), bit10);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Float16), bit16);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Float64), float64Support);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Int64), int64Support);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInRegisters), 32); 
            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxOutRegisters), 32); 
            cap.SetShaderCap<uint>(nameof(ShaderStageCapabilities.MaxInResources), 128);

            // Stage specific settings
            cap.PixelShader.MaxOutResources = 8;
        }

        /// <summary>
        /// See: https://learn.microsoft.com/en-us/windows/win32/api/d3d12/ne-d3d12-d3d12_feature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="feature"></param>
        /// <param name="pData"></param>
        private void GetFeatureSupport<T>(ID3D12Device10* device, Feature feature, T* pData) where T : unmanaged
        {
            uint sizeOf = (uint)sizeof(T);
            HResult r = device->CheckFeatureSupport(feature, pData, sizeOf);
            if (!CheckResult(r))
            {
                string valName = feature.ToString().Replace("Features", "").Replace("Feature", "");
                _renderer.Log.Error($"Failed to retrieve '{valName}' features. Code: {r}");
            }
        }

        /// <summary>
        /// See: https://learn.microsoft.com/en-us/windows/win32/api/d3d12/ne-d3d12-d3d12_feature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        private T GetFeatureSupport<T>(ID3D12Device10* device, Feature feature) where T : unmanaged
        {
            T data = new T();
            GetFeatureSupport(device, feature, &data);
            return data;
        }
    }
}
