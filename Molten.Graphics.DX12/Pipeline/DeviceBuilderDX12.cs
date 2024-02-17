using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Molten.Graphics.DX12;

internal unsafe class DeviceBuilderDX12
{
    RendererDX12 _renderer;
    D3D12 _api;

    readonly D3DFeatureLevel[] _featureLevels = [   
        D3DFeatureLevel.Level122,
        D3DFeatureLevel.Level121,
        D3DFeatureLevel.Level120,
        D3DFeatureLevel.Level111,
        D3DFeatureLevel.Level110
    ];

    internal DeviceBuilderDX12(D3D12 api, RendererDX12 renderer)
    {
        _renderer = renderer;
        _api = api;
    }

    internal HResult CreateDevice(
        DeviceDXGI device,
        out ID3D12Device10* d3dDevice)
    {
        d3dDevice = null;

        IUnknown* ptrAdapter = (IUnknown*)device.Adapter;
        void* ptr = null;

        Guid guidDevice = ID3D12Device.Guid;
        HResult r = _api.CreateDevice(ptrAdapter, _featureLevels.Last(), &guidDevice, &ptr);

        if (!r.IsFailure)
            d3dDevice = (ID3D12Device10*)ptr;

        return r;
    }

    internal void GetCapabilities(GraphicsSettings settings, DeviceDXGI device)
    {
        GraphicsCapabilities cap = device.Capabilities;
        CapabilitiesDX12 features = (device as DeviceDX12).CapabilitiesDX12;

        ID3D12Device10* ptrDevice = null;
        HResult r = CreateDevice(device, out ptrDevice);
        if(!_renderer.Log.CheckResult(r, () => $"Failed to detect capabilities for adapter '{device.Name}'"))
            return;

        FeatureDataFeatureLevels dataFeatures = new FeatureDataFeatureLevels();
        fixed (D3DFeatureLevel* ptrLevels = _featureLevels)
        {
            dataFeatures.NumFeatureLevels = (uint)_featureLevels.Length;
            dataFeatures.PFeatureLevelsRequested = ptrLevels;
            GetFeatureSupport(ptrDevice, Feature.FeatureLevels, &dataFeatures);
        }

        switch (dataFeatures.MaxSupportedFeatureLevel)
        {
            case D3DFeatureLevel.Level122:
                cap.Api = GraphicsApi.DirectX12_2;
                cap.UnorderedAccessBuffers.MaxSlots = D3D12.UavSlotCount;
                cap.MaxShaderModel = ShaderModel.Model6_0;
                break;

            case D3DFeatureLevel.Level121:
                cap.Api = GraphicsApi.DirectX12_1;
                cap.UnorderedAccessBuffers.MaxSlots = D3D12.UavSlotCount;
                cap.MaxShaderModel = ShaderModel.Model6_0;
                break;

            case D3DFeatureLevel.Level120:
                cap.Api = GraphicsApi.DirectX12_0;
                cap.UnorderedAccessBuffers.MaxSlots = D3D12.UavSlotCount;
                cap.MaxShaderModel = ShaderModel.Model5_1;
                break;

            case D3DFeatureLevel.Level111:
                cap.Api = GraphicsApi.DirectX11_1;
                cap.UnorderedAccessBuffers.MaxSlots = D3D12.UavSlotCount;
                cap.MaxShaderModel = ShaderModel.Model5_1;
                break;

            case D3DFeatureLevel.Level110:
                cap.Api = GraphicsApi.DirectX11_0;
                cap.UnorderedAccessBuffers.MaxSlots = D3D12.PSCSUavRegisterCount;
                cap.MaxShaderModel = ShaderModel.Model5_1;
                break;
        }

        // Check if a lower shader model is supported instead of API's model.
        FeatureDataShaderModel maxSM = new FeatureDataShaderModel(cap.MaxShaderModel.ToApi());
        GetFeatureSupport(ptrDevice, Feature.ShaderModel, &maxSM);
        cap.MaxShaderModel = maxSM.HighestShaderModel.FromApi();

        FeatureDataD3D12Options features12_0 = GetFeatureSupport<FeatureDataD3D12Options>(ptrDevice, Feature.D3D12Options);
        FeatureDataD3D12Options1 features12_1 = GetFeatureSupport<FeatureDataD3D12Options1>(ptrDevice, Feature.D3D12Options1);
        FeatureDataD3D12Options2 features12_2 = GetFeatureSupport<FeatureDataD3D12Options2>(ptrDevice, Feature.D3D12Options2);
        FeatureDataD3D12Options3 features12_3 = GetFeatureSupport<FeatureDataD3D12Options3>(ptrDevice, Feature.D3D12Options3);
        FeatureDataD3D12Options4 features12_4 = GetFeatureSupport<FeatureDataD3D12Options4>(ptrDevice, Feature.D3D12Options4);
        FeatureDataD3D12Options5 features12_5 = GetFeatureSupport<FeatureDataD3D12Options5>(ptrDevice, Feature.D3D12Options5); // Raytracing starts here.
        FeatureDataD3D12Options6 features12_6 = GetFeatureSupport<FeatureDataD3D12Options6>(ptrDevice, Feature.D3D12Options6);
        FeatureDataD3D12Options7 features12_7 = GetFeatureSupport<FeatureDataD3D12Options7>(ptrDevice, Feature.D3D12Options7); // Variable shading rate starts here.
        FeatureDataD3D12Options8 features12_8 = GetFeatureSupport<FeatureDataD3D12Options8>(ptrDevice, Feature.D3D12Options8);
        FeatureDataD3D12Options9 features12_9 = GetFeatureSupport<FeatureDataD3D12Options9>(ptrDevice, Feature.D3D12Options9);
        FeatureDataD3D12Options10 features12_10 = GetFeatureSupport<FeatureDataD3D12Options10>(ptrDevice, Feature.D3D12Options10);
        FeatureDataD3D12Options11 features12_11 = GetFeatureSupport<FeatureDataD3D12Options11>(ptrDevice, Feature.D3D12Options11);
        FeatureDataD3D12Options12 features12_12 = GetFeatureSupport<FeatureDataD3D12Options12>(ptrDevice, Feature.D3D12Options12);
        FeatureDataD3D12Options13 features12_13 = GetFeatureSupport<FeatureDataD3D12Options13>(ptrDevice, Feature.D3D12Options13);

        cap.MaxTexture1DSize = D3D12.ReqTexture1DUDimension;
        cap.MaxTexture2DSize = D3D12.ReqTexture2DUOrVDimension;
        cap.MaxTexture3DSize = D3D12.ReqTexture3DUVOrWDimension;
        cap.MaxTextureCubeSize = 16384;
        cap.MaxAnisotropy = D3D12.MaxMaxanisotropy;
        cap.BlendLogicOp = features12_0.OutputMergerLogicOp > 0;
        cap.MaxShaderSamplers = D3D12.CommonshaderSamplerSlotCount;
        cap.OcclusionQueries = true;
        cap.HardwareInstancing = true;
        cap.MaxTextureArraySlices = D3D12.ReqTexture2DArrayAxisDimension;
        cap.TextureCubeArrays = true;
        cap.NonPowerOfTwoTextures = true;
        cap.MaxAllocatedSamplers = 4096;                // D3D11_REQ_SAMPLER_OBJECT_COUNT_PER_DEVICE (4096) - Total number of sampler objects per context
        cap.MaxPrimitiveCount = uint.MaxValue;          // (2^32) – 1 = uint.maxValue (4,294,967,295)
        cap.RasterizerOrderViews = features12_0.ROVsSupported;
        cap.ConservativeRasterization = (ConservativeRasterizationLevel)features12_0.ConservativeRasterizationTier;

        // NOTE:You can bind up to 14 constant buffers per pipeline stage (2 additional slots are reserved for internal use).
        // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-constants
        cap.ConstantBuffers.MaxSlots = D3D12.CommonshaderConstantBufferApiSlotCount;              // 15 (1 reserved for immediate constant buffer).
        cap.ConstantBuffers.MaxElements = D3D12.ReqConstantBufferElementCount;
        cap.ConstantBuffers.MaxBytes = cap.ConstantBuffers.MaxElements * (4 * sizeof(float)); // Max of four float components per element.

        cap.VertexBuffers.MaxSlots = D3D12.IAVertexInputResourceSlotCount;
        cap.VertexBuffers.MaxElementsPerVertex = D3D12.StandardVertexElementCount;
        cap.VertexBuffers.MaxElements = uint.MaxValue;  // (2^32) – 1 = uint.maxValue (4,294,967,295)

        DetectShaderStages(cap, 
            features12_0.MinPrecisionSupport, 
            features12_0.DoublePrecisionFloatShaderOps, 
            features12_1.Int64ShaderOps);

        cap.PixelShader.MaxOutputTargets = D3D12.PSOutputRegisterCount;

        FeatureDataRootSignature rootSig = new FeatureDataRootSignature();
        rootSig.HighestVersion = RendererDX12.MAX_ROOT_SIG_VERSION;
        GetFeatureSupport(ptrDevice, Feature.RootSignature, &rootSig);
        features.RootSignatureVersion = rootSig.HighestVersion;

        NativeUtil.ReleasePtr(ref ptrDevice);
    }

    private void DetectShaderStages(GraphicsCapabilities cap, 
        ShaderMinPrecisionSupport minPrecision,
        bool float64Support, bool int64Support)
    {
        bool bit10 = (minPrecision & ShaderMinPrecisionSupport.Support10Bit) == ShaderMinPrecisionSupport.Support10Bit;
        bool bit16 = (minPrecision & ShaderMinPrecisionSupport.Support16Bit) == ShaderMinPrecisionSupport.Support16Bit;

        cap.SetShaderCap(nameof(ShaderStageCapabilities.Float10), bit10);
        cap.SetShaderCap(nameof(ShaderStageCapabilities.Float16), bit16);
        cap.SetShaderCap(nameof(ShaderStageCapabilities.Float64), float64Support);
        cap.SetShaderCap(nameof(ShaderStageCapabilities.Int64), int64Support);
        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInRegisters), 32U); 
        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxOutRegisters), 32U); 
        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInResources), 128U);

        // Pixel/fragment-specific capabilities
        // NOTE: When the pipeline is configured without a geometry shader, a pixel shader is limited to 16, 32-bit, 4-component inputs.
        //       Otherwise, a pixel shader can take up to 32, 32-bit, 4-component inputs.
        cap.PixelShader.MaxOutputTargets = D3D12.PSOutputRegisterCount;
        cap.PixelShader.MaxOutRegisters = D3D12.PSOutputRegisterCount;

        // Compute-specific capabilities
        cap.Compute.MaxGroupCountX = D3D12.CSDispatchMaxThreadGroupsPerDimension;
        cap.Compute.MaxGroupCountY = D3D12.CSDispatchMaxThreadGroupsPerDimension;
        cap.Compute.MaxGroupCountZ = D3D12.CSDispatchMaxThreadGroupsPerDimension;
        cap.Compute.MaxGroupSizeX = D3D12.CSThreadGroupMaxX;
        cap.Compute.MaxGroupSizeY = D3D12.CSThreadGroupMaxY;
        cap.Compute.MaxGroupSizeZ = D3D12.CSThreadGroupMaxZ;
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
        if (!_renderer.Log.CheckResult(r))
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
