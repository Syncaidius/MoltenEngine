﻿using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D11.Feature;

namespace Molten.Graphics
{

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

        internal HResult CreateDevice(
            DisplayAdapterDXGI adapter, 
            DeviceCreationFlags flags,
            out ID3D11Device5* device, 
            out ID3D11DeviceContext4* context)
        {
            IDXGIAdapter* ptrAdapter = (IDXGIAdapter*)adapter.Native;
            ID3D11Device* ptrDevice = null;
            ID3D11DeviceContext* ptrContext = null;

            HResult r = _api.CreateDevice(ptrAdapter,
                D3DDriverType.Unknown,
                0,
                (uint)flags,
                ref _featureLevels[0], 
                (uint)_featureLevels.Length,
                D3D11.SdkVersion,
                &ptrDevice,
                null,
                &ptrContext);

            Guid dev5Guid = ID3D11Device5.Guid;
            void* ptrDevice5 = null;
            ptrDevice->QueryInterface(&dev5Guid, &ptrDevice5);
            device = (ID3D11Device5*)ptrDevice5;

            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptrCxt4 = null;
            ptrContext->QueryInterface(&cxt4Guid, &ptrCxt4);
            context = (ID3D11DeviceContext4*)ptrCxt4;

            return r;
        }

        internal HResult CreateHeadlessDevice(
            DisplayAdapterDXGI adapter,
            DeviceCreationFlags flags,
            out ID3D11Device5* device)
        {
            IDXGIAdapter* ptrAdapter = (IDXGIAdapter*)adapter.Native;
            ID3D11Device* ptrDevice = null;

            HResult r = _api.CreateDevice(ptrAdapter,
                D3DDriverType.Unknown,
                0,
                (uint)flags,
                ref _featureLevels[0],
                (uint)_featureLevels.Length,
                D3D11.SdkVersion,
                &ptrDevice,
                null,
                null);

            Guid dev5Guid = ID3D11Device5.Guid;
            void* ptrDevice5 = null;
            ptrDevice->QueryInterface(&dev5Guid, &ptrDevice5);
            device = (ID3D11Device5*)ptrDevice5;

            return r;
        }

        internal GraphicsCapabilities GetCapabilities(DisplayAdapterDXGI adapter)
        {
            // DX11 resource limits: https://msdn.microsoft.com/en-us/library/windows/desktop/ff819065%28v=vs.85%29.aspx
            GraphicsCapabilities cap = new GraphicsCapabilities();
            ID3D11Device5* device = null;
            CreateHeadlessDevice(adapter, DeviceCreationFlags.None, out device);

            D3DFeatureLevel featureLevel = device->GetFeatureLevel();
            switch (featureLevel)
            {
                case D3DFeatureLevel.Level111:
                    cap.Api = GraphicsApi.DirectX11_1;
                    cap.UnorderedAccessBuffers.MaxSlots = 64; // D3D11_1_UAV_SLOT_COUNT = 64
                    break;

                case D3DFeatureLevel.Level110:
                    cap.Api = GraphicsApi.DirectX11_0;
                    cap.UnorderedAccessBuffers.MaxSlots = 8;  // D3D11_PS_CS_UAV_REGISTER_COUNT = 8
                    break;
            }

            //Compute = new GraphicsComputeFeatures(this);
            FeatureDataD3D11Options features11_0 = GetFeatureSupport<FeatureDataD3D11Options>(device, Feature.D3D11Options);
            FeatureDataD3D11Options1 features11_1 = GetFeatureSupport<FeatureDataD3D11Options1>(device, Feature.D3D11Options1);
            FeatureDataD3D11Options2 features11_2 = GetFeatureSupport<FeatureDataD3D11Options2>(device, Feature.D3D11Options2);
            FeatureDataD3D11Options3 features11_3 = GetFeatureSupport<FeatureDataD3D11Options3>(device, Feature.D3D11Options3);
            FeatureDataD3D11Options4 features11_4 = GetFeatureSupport<FeatureDataD3D11Options4>(device, Feature.D3D11Options4);
            FeatureDataD3D11Options5 features11_5 = GetFeatureSupport<FeatureDataD3D11Options5>(device, Feature.D3D11Options5);

            //CounterInfo cInfo = new CounterInfo();
            //device->CheckCounterInfo(&cInfo);
            //CounterSupport = cInfo;

            cap.MaxShaderModel = ShaderModel.Model5_0;
            cap.MaxTexture1DSize = 16384;
            cap.MaxTexture2DSize = 16384;
            cap.MaxTexture3DSize = 2048;
            cap.MaxTextureCubeSize = 16384;
            cap.MaxAnisotropy = 16;
            cap.BlendLogicOp = features11_0.OutputMergerLogicOp > 0;
            cap.MaxShaderSamplers = 16;
            cap.OcclusionQueries = true;
            cap.HardwareInstancing = true;
            cap.MaxTextureArraySlices = 2048;               // D3D11_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION (2048 array slices)
            cap.TextureCubeArrays = true;
            cap.NonPowerOfTwoTextures = true;
            cap.MaxAllocatedSamplers = 4096;                // D3D11_REQ_SAMPLER_OBJECT_COUNT_PER_DEVICE (4096) - Total number of sampler objects per context
            cap.MaxPrimitiveCount = uint.MaxValue;          // (2^32) – 1 = uint.maxValue (4,294,967,295)

            cap.VertexBuffers.MaxSlots = 32;                // D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT = 32;
            cap.VertexBuffers.MaxElementsPerVertex = 32;    // D3D11_STANDARD_VERTEX_ELEMENT_COUNT = 32;
            cap.VertexBuffers.MaxElements = uint.MaxValue;  // (2^32) – 1 = uint.maxValue (4,294,967,295)

            cap.ConstantBuffers.MaxSlots = 15;              // D3D11_COMMONSHADER_CONSTANT_BUFFER_HW_SLOT_COUNT  = 15 (+1 for immediate constant buffer).
            cap.ConstantBuffers.MaxElements = 4096;         // D3D11_REQ_CONSTANT_BUFFER_ELEMENT_COUNT = 4096
            cap.ConstantBuffers.MaxBytes = cap.ConstantBuffers.MaxElements * (4 * sizeof(float)); // Max of four float components per element.

            // See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch
            cap.Compute.MaxGroupCountX = 65535;
            cap.Compute.MaxGroupCountY = 65535;
            cap.Compute.MaxGroupCountZ = 65535;

            // See: https://learn.microsoft.com/en-us/windows/win32/direct3d11/direct3d-11-advanced-stages-compute-shader
            cap.Compute.MaxGroupSizeX = 1024;               // Shader Model 5.0 - The maximum number of threads is limited to D3D11_CS_THREAD_GROUP_MAX_THREADS_PER_GROUP (1024) per group.
            cap.Compute.MaxGroupSizeY = 1024;
            cap.Compute.MaxGroupSizeZ = 64;                 // The Z dimension of numthreads is limited to D3D11_CS_THREAD_GROUP_MAX_Z (64).;

            /*            MaxVolumeExtent = 2048;
            MaxTextureRepeat = 16384;
            MaxPrimitiveCount = (uint)(Math.Pow(2, 32) - 1);
            MaxUnorderedAccessViews = 8;
            MaxIndexBufferSlots = 1;
            MaxVertexBufferSlots = 32;     */


            DetectShaderStages(device, cap, featureLevel);

            /*FeatureDataThreading fThreadData = GetFeatureSupport<FeatureDataThreading>(device, Feature.Threading);
            ConcurrentResources = fThreadData.DriverConcurrentCreates > 0;
            CommandListSupport = fThreadData.DriverCommandLists > 0 ?
                DX11CommandListSupport.Supported :
                DX11CommandListSupport.Emulated;*/


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


            SilkUtil.ReleasePtr(ref device);
            return cap;
        }

        private void DetectShaderStages(ID3D11Device5* device, GraphicsCapabilities cap, D3DFeatureLevel featureLevel)
        {
            FeatureDataDoubles fData = GetFeatureSupport<FeatureDataDoubles>(device, Feature.Doubles);
            bool dp = fData.DoublePrecisionFloatShaderOps > 0;

            // DirectX 11.1 or higher precision features
            if (featureLevel >= D3DFeatureLevel.Level111)
            {
                FeatureDataShaderMinPrecisionSupport mData = GetFeatureSupport<FeatureDataShaderMinPrecisionSupport>(device, Feature.ShaderMinPrecisionSupport);
                ShaderMinPrecisionSupport all = (ShaderMinPrecisionSupport)mData.AllOtherShaderStagesMinPrecision;
                bool all10Bit = (all & ShaderMinPrecisionSupport.Precision10Bit) == ShaderMinPrecisionSupport.Precision10Bit;
                bool all16Bit = (all & ShaderMinPrecisionSupport.Precision16Bit) == ShaderMinPrecisionSupport.Precision16Bit;
                ShaderMinPrecisionSupport pixel = (ShaderMinPrecisionSupport)mData.PixelShaderMinPrecision;

                cap.SetShaderCap(nameof(ShaderStageCapabilities.Float10), all10Bit);
                cap.SetShaderCap(nameof(ShaderStageCapabilities.Float16), all16Bit);
                cap.PixelShader.Float10 = (pixel & ShaderMinPrecisionSupport.Precision10Bit) == ShaderMinPrecisionSupport.Precision10Bit;
                cap.PixelShader.Float16 = (pixel & ShaderMinPrecisionSupport.Precision16Bit) == ShaderMinPrecisionSupport.Precision16Bit;
            }

            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInRegisters), 32); // D3D11_VS/GS/PS_INPUT_REGISTER_COUNT (32)
            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxOutRegisters), 32); // D3D11_VS/GS/DS_OUTPUT_REGISTER_COUNT (32)
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Float64), dp);
            cap.SetShaderCap<uint>(nameof(ShaderStageCapabilities.MaxInResources), 128); // D3D11_COMMONSHADER_INPUT_RESOURCE_REGISTER_COUNT (128)

            // Stage specific settings
            cap.PixelShader.MaxOutResources = 8;
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
}
