using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D11.Feature;

namespace Molten.Graphics.DX11;


public unsafe class DeviceBuilderDX11
{
    RendererDX11 _renderer;
    D3D11 _api;
    D3DFeatureLevel[] _featureLevels;

    internal DeviceBuilderDX11(D3D11 api, RendererDX11 renderer, params D3DFeatureLevel[] featureLevels)
    {
        _renderer = renderer;
        _api = api;
        _featureLevels = featureLevels;
    }

    private DeviceCreationFlags GetFlags(GraphicsSettings settings)
    {
        DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;

        if (_renderer.Settings.Graphics.EnableDebugLayer)
        {
            _renderer.Log.WriteLine("Renderer debug layer enabled");
            flags |= DeviceCreationFlags.Debug;
        }

        return flags;
    }

    internal HResult CreateDevice(DeviceDXGI adapter, out ID3D11Device5* device)
    {
        ID3D11DeviceContext4* nullContext = null;
        return CreateDevice(adapter, out device, out nullContext, true);
    }

    internal HResult CreateDevice(
        DeviceDXGI adapter,
        out ID3D11Device5* device,
        out ID3D11DeviceContext4* context, bool headless = false)
    {
        context = null;
        device = null;

        DeviceCreationFlags flags = GetFlags(_renderer.Settings.Graphics);
        IDXGIAdapter* ptrAdapter = (IDXGIAdapter*)adapter.Adapter;
        ID3D11Device* ptrDevice = null;
        ID3D11DeviceContext* ptrContext = null;
        ID3D11DeviceContext** ptrContextRef = &ptrContext;

        if (headless)
            ptrContextRef = null;

        D3DDriverType type = D3DDriverType.Unknown;
        if (adapter.Vendor == DeviceVendor.Intel)
            type = D3DDriverType.Hardware;

        HResult r;
        fixed (D3DFeatureLevel* ptrFeatureLevels = &_featureLevels[0])
        {
            r = _api.CreateDevice(ptrAdapter,
                type,
                0,
                (uint)flags,
                ptrFeatureLevels,
                (uint)_featureLevels.Length,
                D3D11.SdkVersion,
                &ptrDevice,
                null,
                ptrContextRef);
        }

        if (!r.IsFailure)
        {
            Guid dev5Guid = ID3D11Device5.Guid;
            void* ptrDevice5 = null;
            ptrDevice->QueryInterface(&dev5Guid, &ptrDevice5);
            device = (ID3D11Device5*)ptrDevice5;
        }

        if (!headless)
        {
            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptrCxt4 = null;
            ptrContext->QueryInterface(&cxt4Guid, &ptrCxt4);
            context = (ID3D11DeviceContext4*)ptrCxt4;
        }

        return r;
    }

    internal void GetCapabilities(GraphicsSettings settings, DeviceDXGI adapter)
    {
        // DX11 resource limits: https://msdn.microsoft.com/en-us/library/windows/desktop/ff819065%28v=vs.85%29.aspx
        GraphicsCapabilities cap = adapter.Capabilities;
        ID3D11Device5* device = null;
        HResult r = CreateDevice(adapter, out device);
        if (r.IsFailure)
        {
            _renderer.Log.Error($"Failed to detect capabilities for adapter '{adapter.Name}'");
            return;
        }

        D3DFeatureLevel featureLevel = device->GetFeatureLevel();
        switch (featureLevel)
        {
            case D3DFeatureLevel.Level111:
                cap.Api = GraphicsApi.DirectX11_1;
                cap.UnorderedAccessBuffers.MaxSlots = D3D11.D3D111UavSlotCount;
                break;

            case D3DFeatureLevel.Level110:
                cap.Api = GraphicsApi.DirectX11_0;
                cap.UnorderedAccessBuffers.MaxSlots = D3D11.PSCSUavRegisterCount;
                break;
        }

        //Compute = new GraphicsComputeFeatures(this);
        FeatureDataD3D11Options features11_0 = GetFeatureSupport<FeatureDataD3D11Options>(device, Feature.D3D11Options);
        FeatureDataD3D11Options1 features11_1 = GetFeatureSupport<FeatureDataD3D11Options1>(device, Feature.D3D11Options1);
        FeatureDataD3D11Options2 features11_2 = GetFeatureSupport<FeatureDataD3D11Options2>(device, Feature.D3D11Options2);
        FeatureDataD3D11Options3 features11_3 = GetFeatureSupport<FeatureDataD3D11Options3>(device, Feature.D3D11Options3);
        FeatureDataD3D11Options4 features11_4 = GetFeatureSupport<FeatureDataD3D11Options4>(device, Feature.D3D11Options4);
        FeatureDataD3D11Options5 features11_5 = GetFeatureSupport<FeatureDataD3D11Options5>(device, Feature.D3D11Options5);
        FeatureDataThreading feature_threading = GetFeatureSupport<FeatureDataThreading>(device, Feature.Threading);

        //CounterInfo cInfo = new CounterInfo();
        //device->CheckCounterInfo(&cInfo);
        //CounterSupport = cInfo;

        cap.Flags |= features11_0.OutputMergerLogicOp > 0 ? GraphicsCapabilityFlags.BlendLogicOp : GraphicsCapabilityFlags.None;
        cap.Flags |= GraphicsCapabilityFlags.NonPowerOfTwoTextures;
        cap.Flags |= GraphicsCapabilityFlags.OcculsionQueries;
        cap.Flags |= GraphicsCapabilityFlags.HardwareInstancing;
        cap.Flags |= GraphicsCapabilityFlags.TextureCubeArrays;

        cap.MaxShaderModel = ShaderModel.Model5_0;
        cap.MaxTexture1DSize = D3D11.ReqTexture1DUDimension;
        cap.MaxTexture2DSize = D3D11.ReqTexture2DUOrVDimension;
        cap.MaxTexture3DSize = D3D11.ReqTexture3DUVOrWDimension;
        cap.MaxTextureCubeSize = D3D11.ReqTexturecubeDimension;
        cap.MaxAnisotropy = D3D11.MaxMaxanisotropy;
        cap.MaxShaderSamplers = D3D11.CommonshaderSamplerSlotCount;
        cap.MaxTextureArraySlices = D3D11.ReqTexture2DArrayAxisDimension;
        cap.MaxAllocatedSamplers = D3D11.ReqSamplerObjectCountPerDevice;
        cap.MaxPrimitiveCount = uint.MaxValue;          // (2^32) – 1 = uint.maxValue (4,294,967,295)

        // NOTE:You can bind up to 14 constant buffers per pipeline stage (2 additional slots are reserved for internal use).
        // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-constants
        cap.ConstantBuffers.MaxSlots = D3D11.CommonshaderConstantBufferApiSlotCount;              // 15 (1 reserved for immediate constant buffer).
        cap.ConstantBuffers.MaxElements = D3D11.ReqConstantBufferElementCount;
        cap.ConstantBuffers.MaxBytes = cap.ConstantBuffers.MaxElements * (4 * sizeof(float)); // Max of four float components per element.

        cap.VertexBuffers.MaxSlots = D3D11.IAVertexInputResourceSlotCount;
        cap.VertexBuffers.MaxElementsPerVertex = D3D11.StandardVertexElementCount;
        cap.VertexBuffers.MaxElements = uint.MaxValue;  // (2^32) – 1 = uint.maxValue (4,294,967,295)

        // See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch
        cap.Compute.MaxGroupCountX = D3D11.CSDispatchMaxThreadGroupsPerDimension;
        cap.Compute.MaxGroupCountY = D3D11.CSDispatchMaxThreadGroupsPerDimension;
        cap.Compute.MaxGroupCountZ = D3D11.CSDispatchMaxThreadGroupsPerDimension;

        // See: https://learn.microsoft.com/en-us/windows/win32/direct3d11/direct3d-11-advanced-stages-compute-shader
        cap.Compute.MaxGroupSizeX = D3D11.CSThreadGroupMaxX;
        cap.Compute.MaxGroupSizeY = D3D11.CSThreadGroupMaxY;
        cap.Compute.MaxGroupSizeZ = D3D11.CSThreadGroupMaxZ;

        cap.CommandSets.Add(new SupportedCommandSet()
        {
            MaxQueueCount = 1,
            CapabilityFlags = CommandSetCapabilityFlags.Graphics |
            CommandSetCapabilityFlags.Compute |
            CommandSetCapabilityFlags.TransferCopy,
            TimeStampBits = 64,  // Queries of type D3D11_QUERY_TIMESTAMP always return UINT64
        });

        
        cap.Flags |= feature_threading.DriverConcurrentCreates > 0 ? GraphicsCapabilityFlags.ConcurrentResourceCreation : GraphicsCapabilityFlags.None;
        cap.DeferredCommandLists = feature_threading.DriverCommandLists > 0 ? CommandListSupport.Supported : CommandListSupport.Emulated;

        /* MaxTextureRepeat = 16384;
        MaxPrimitiveCount = (uint)(Math.Pow(2, 32) - 1);
        MaxIndexBufferSlots = 1;     */

        DetectShaderStages(device, cap, featureLevel);

        //            FeatureDataD3D10XHardwareOptions fData =
        //                _features.GetFeatureSupport<FeatureDataD3D10XHardwareOptions>(Feature.D3D10XHardwareOptions);

        //            Supported = fData.ComputeShadersPlusRawAndStructuredBuffersViaShader4X > 0;
        //        }

        //        /// <summary>Returns all of the supported compute shader features for the provided <see cref="Format"/>.</summary>
        //        /// <param name="format">The format of which to retrieve compute shader support.</param>
        //        /// <returns>Returns <see cref="FormatSupport2"/> flags containing compute feature support for the specified <see cref="Format"/>.</returns>
        //        public unsafe FormatSupport2 GetFormatSupport(Format format)
        //        {
        //            FeatureDataFormatSupport2 pData = new FeatureDataFormatSupport2()
        //            {
        //                InFormat = format,
        //            };

        //            _features.GetFeatureSupport(Feature.FormatSupport2, &pData);

        //            return (FormatSupport2)pData.OutFormatSupport2;
        //        }

        NativeUtil.ReleasePtr(ref device);
    }

    private void DetectShaderStages(ID3D11Device5* device, GraphicsCapabilities cap, D3DFeatureLevel featureLevel)
    {
        FeatureDataDoubles fData = GetFeatureSupport<FeatureDataDoubles>(device, Feature.Doubles);

        // DirectX 11.1 or higher precision features
        if (featureLevel >= D3DFeatureLevel.Level111)
        {
            FeatureDataShaderMinPrecisionSupport mData = GetFeatureSupport<FeatureDataShaderMinPrecisionSupport>(device, Feature.ShaderMinPrecisionSupport);
            ShaderMinPrecisionSupport all = (ShaderMinPrecisionSupport)mData.AllOtherShaderStagesMinPrecision;
            ShaderMinPrecisionSupport pixel = (ShaderMinPrecisionSupport)mData.PixelShaderMinPrecision;

            if ((all & ShaderMinPrecisionSupport.Precision10Bit) == ShaderMinPrecisionSupport.Precision10Bit)
                cap.AddShaderCap(ShaderCapabilityFlags.Float10);

            if ((all & ShaderMinPrecisionSupport.Precision16Bit) == ShaderMinPrecisionSupport.Precision16Bit)
                cap.AddShaderCap(ShaderCapabilityFlags.Float16);

            // Pixel shader precision support is separate from the rest of the shader stages.
            // If pixel shader does not support a precision, then remove the capability flag that may have been set in the calls above.
            if ((pixel & ShaderMinPrecisionSupport.Precision10Bit) != ShaderMinPrecisionSupport.Precision10Bit)
                cap.PixelShader.Flags &= ~ShaderCapabilityFlags.Float16;

            if ((pixel & ShaderMinPrecisionSupport.Precision16Bit) != ShaderMinPrecisionSupport.Precision16Bit)
                cap.PixelShader.Flags &= ~ShaderCapabilityFlags.Float16;

            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxUnorderedAccessSlots), cap.UnorderedAccessBuffers.MaxSlots);
        }
        else
        {
            cap.Compute.MaxUnorderedAccessSlots = cap.UnorderedAccessBuffers.MaxSlots;
        }


        if (fData.DoublePrecisionFloatShaderOps > 0)
            cap.AddShaderCap(ShaderCapabilityFlags.Float64);

        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInRegisters), 32U); // D3D11_VS/GS/PS_INPUT_REGISTER_COUNT (32)
        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxOutRegisters), 32U); // D3D11_VS/GS/DS_OUTPUT_REGISTER_COUNT (32)
        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInResources), 128U); // D3D11_COMMONSHADER_INPUT_RESOURCE_REGISTER_COUNT (128)

        // Stage specific settings
        cap.PixelShader.MaxOutputTargets = 8;
    }

    internal void GetFeatureSupport<T>(ID3D11Device5* device, Feature feature, T* pData) where T : unmanaged
    {
        uint sizeOf = (uint)sizeof(T);
        device->CheckFeatureSupport(feature, pData, sizeOf);
    }

    internal T GetFeatureSupport<T>(ID3D11Device5* device, Feature feature) where T : unmanaged
    {
        uint sizeOf = (uint)sizeof(T);
        T data = new T();
        device->CheckFeatureSupport(feature, &data, sizeOf);
        return data;
    }

    /// <summary>Returns a set of format support flags for the specified format, or none if format support checks are not supported.</summary>
    /// <param name="format"></param>
    /// <returns></returns>
    internal unsafe FormatSupport GetFormatSupport(ID3D11Device5* device, Format format)
    {
        uint fData = 0;
        device->CheckFormatSupport(format, &fData);

        return (FormatSupport)fData;
    }

    /// <summary>Returns true if core features of the provided format are supported by the graphics device.</summary>
    /// <param name="format">The format to check for support on the device.</param>
    /// <returns></returns>
    internal bool IsFormatSupported(ID3D11Device5* device, Format format)
    {
        FormatSupport support = GetFormatSupport(device, format);

        bool supported = support.HasFlag(FormatSupport.Texture2D | 
            FormatSupport.Texture3D | 
            FormatSupport.CpuLockable |
            FormatSupport.ShaderSample | 
            FormatSupport.ShaderLoad | 
            FormatSupport.Mip);

        return supported;
    }

    ///// <summary>Returns the number of supported quality levels for the specified format and sample count.</summary>
    ///// <param name="format">The format to test quality levels against.</param>
    ///// <param name="sampleCount">The sample count to test against.</param>
    ///// <returns></returns>
    //internal MSAASupport GetMSAASupport(ID3D11Device5* device, Format format, AntiAliasLevel aaLevel)
    //{
    //    uint numQualityLevels = (uint)MSAASupport.FixedOnly;
    //    HResult hr = device->CheckMultisampleQualityLevels(format, (uint)aaLevel, &numQualityLevels);
    //    return hr.IsSuccess ? (MSAASupport)numQualityLevels: MSAASupport.NotSupported;
    //}
}
