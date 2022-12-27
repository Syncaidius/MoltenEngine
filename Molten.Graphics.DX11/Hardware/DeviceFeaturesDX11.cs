using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D11.Feature;

namespace Molten.Graphics
{

    internal unsafe class DeviceFeaturesDX11 : GraphicsCapabilities
    {
        ID3D11Device5* _device;

        // DX11 resource limits: https://msdn.microsoft.com/en-us/library/windows/desktop/ff819065%28v=vs.85%29.aspx

        internal DeviceFeaturesDX11(ID3D11Device5* d3dDevice)
        {
            _device = d3dDevice;
            FeatureLevel = _device->GetFeatureLevel();

            Compute = new GraphicsComputeFeatures(this);
            MiscFeatures = GetFeatureSupport<FeatureDataD3D11Options>(Feature.D3D11Options);

            CounterInfo cInfo = new CounterInfo();
            _device->CheckCounterInfo(&cInfo);
            CounterSupport = cInfo;

            //calculate level-specific features.
            switch (FeatureLevel)
            {
                default:
                case D3DFeatureLevel.D3DFeatureLevel110:
                    MaxTexture1DSize = 16384;
                    MaxTexture2DSize = 16384;
                    MaxTexture3DSize = 16384;
                    MaxTextureCubeSize = 16384;

                    Sampler.MaxAnisotropy = 16;

                    SimultaneousRenderSurfaces = 8;
                    MaxVolumeExtent = 2048;
                    MaxTextureRepeat = 16384;
                    MaxPrimitiveCount = (uint)(Math.Pow(2, 32) - 1);
                    MaxInputResourceSlots = 128;
                    MaxSamplerSlots = 16;
                    OcclusionQueries = true;
                    HardwareInstancing = true;
                    TextureArrays = true;
                    CubeMapArrays = true;
                    NonPowerOfTwoTextures = true;
                    MaximumShaderModel = ShaderModel.Model5_0;
                    MaxUnorderedAccessViews = 8;
                    MaxIndexBufferSlots = 1;
                    MaxVertexBufferSlots = 32;
                    MaxConstantBufferSlots = 15;                    
                    break;
            }

            DetectShaderPrecisionSupport();

            FeatureDataThreading fThreadData = GetFeatureSupport<FeatureDataThreading>(Feature.Threading);
            ConcurrentResources = fThreadData.DriverConcurrentCreates > 0;
            CommandListSupport = fThreadData.DriverCommandLists > 0 ? 
                DX11CommandListSupport.Supported : 
                DX11CommandListSupport.Emulated;
        }

        private void DetectShaderPrecisionSupport()
        {
            FeatureDataDoubles fData = GetFeatureSupport<FeatureDataDoubles>(Feature.Doubles);
            bool dp = fData.DoublePrecisionFloatShaderOps > 0;

            ShaderMinPrecisionSupport all = ShaderMinPrecisionSupport.None;
            ShaderMinPrecisionSupport pixel = ShaderMinPrecisionSupport.None;

            // DirectX 11.1 or higher precision features
            if (FeatureLevel >= D3DFeatureLevel.Level111)
            {
                FeatureDataShaderMinPrecisionSupport mData = GetFeatureSupport<FeatureDataShaderMinPrecisionSupport>(Feature.ShaderMinPrecisionSupport);
                all = (ShaderMinPrecisionSupport)mData.AllOtherShaderStagesMinPrecision;
                pixel = (ShaderMinPrecisionSupport)mData.PixelShaderMinPrecision;
            }

            SetShaderPrecision(VertexShader, all, dp);
            SetShaderPrecision(GeometryShader, all, dp);
            SetShaderPrecision(HullShader, all, dp);
            SetShaderPrecision(DomainShader, all, dp);
            SetShaderPrecision(Compute, all, dp);
            SetShaderPrecision(PixelShader, pixel, dp);
        }

        private void SetShaderPrecision(ShaderStageCapabilities sCap, ShaderMinPrecisionSupport minPrecision, bool float64)
        {
            sCap.Float10 = (minPrecision & ShaderMinPrecisionSupport.Precision10Bit) == ShaderMinPrecisionSupport.Precision10Bit;
            sCap.Float16 = (minPrecision & ShaderMinPrecisionSupport.Precision16Bit) == ShaderMinPrecisionSupport.Precision16Bit;
            sCap.Float64 = float64;
        }

        internal void GetFeatureSupport<T>(Feature feature, T* pData) where T : unmanaged
        {
            uint sizeOf = (uint)sizeof(T);
            _device->CheckFeatureSupport(feature, pData, sizeOf);
        }

        internal T GetFeatureSupport<T>(Feature feature) where T : unmanaged
        {
            uint sizeOf = (uint)sizeof(T);
            T data = new T();
            _device->CheckFeatureSupport(feature, &data, sizeOf);
            return data;
        }

        /// <summary>Returns a set of format support flags for the specified format, or none if format support checks are not supported.</summary>
        /// <param name="format"></param>
        /// <returns></returns>
        internal unsafe FormatSupport GetFormatSupport(Format format)
        {
            uint fData = 0;
            _device->CheckFormatSupport(format, &fData);

            return (FormatSupport)fData;
        }

        /// <summary>Returns true if core features of the provided format are supported by the graphics device.</summary>
        /// <param name="format">The format to check for support on the device.</param>
        /// <returns></returns>
        internal bool IsFormatSupported(Format format)
        {
            FormatSupport support = GetFormatSupport(format);

            bool supported = support.HasFlag(FormatSupport.Texture2D | 
                FormatSupport.Texture3D | 
                FormatSupport.CpuLockable |
                FormatSupport.ShaderSample | 
                FormatSupport.ShaderLoad | 
                FormatSupport.Mip);

            return supported;
        }



        /// <summary>Returns the number of supported quality levels for the specified format and sample count.</summary>
        /// <param name="format">The format to test quality levels against.</param>
        /// <param name="sampleCount">The sample count to test against.</param>
        /// <returns></returns>
        internal MSAASupport GetMSAASupport(Format format, AntiAliasLevel aaLevel)
        {
            uint numQualityLevels = (uint)MSAASupport.FixedOnly;
            HResult hr = _device->CheckMultisampleQualityLevels(format, (uint)aaLevel, &numQualityLevels);
            return hr.IsSuccess ? (MSAASupport)numQualityLevels: MSAASupport.NotSupported;
        }

        /// <summary>Gets a <see cref="CounterCapabilities>"/> containing details of the device's counter support.</summary>
        internal CounterInfo CounterSupport { get; }

        /// <summary>Gets the <see cref="D3DFeatureLevel"/> of the current device.</summary>
        internal D3DFeatureLevel FeatureLevel { get; }

        internal D3DFeatureLevel MaxFeatureLevel { get; }

        /// <summary>Gets an instance of <see cref="GraphicsComputeFeatures"/> which contains the supported compute features of a <see cref="DeviceDX11"/>.</summary>
        internal GraphicsComputeFeatures Compute { get; private set; }

        internal FeatureDataD3D11Options MiscFeatures { get; private set; }

        /// <summary>Undocumented.</summary>
        internal uint MaxVolumeExtent { get; private set; }

        /// <summary>The maximum number of times a texture is allowed to repeat.</summary>
        internal uint MaxTextureRepeat { get; private set; }


        /// <summary>Gets the maximum number of primitives (triangles) the device can render in a single draw call.</summary>
        internal uint MaxPrimitiveCount { get; private set; }

        /// <summary>Gets the max number of shader resource input slots that the device supports for all shader stages.</summary>
        internal uint MaxInputResourceSlots { get; private set; }

        /// <summary>Gets the max number of sampler slots that the device supports for all shader stages.</summary>
        internal uint MaxSamplerSlots { get; private set; }

        /// <summary>Gets whether or not the device supports occlusion queries.</summary>
        internal bool OcclusionQueries { get; private set; }

        /// <summary>Gets whether or not texture arrays are supported.</summary>
        internal bool TextureArrays { get; private set; }

        /// <summary>Gets whether or not cube map arrays are supported. </summary>
        internal bool CubeMapArrays { get; private set; }

        /// <summary>Gets whether or not non-power of 2 textures are supported.</summary>
        internal bool NonPowerOfTwoTextures { get; private set; }

        /// <summary>Gets the maximum shader model supported by the graphics device.</summary>
        internal ShaderModel MaximumShaderModel { get; private set; }

        /// <summary>Gets the maximum supported number of index buffer slots.</summary>
        internal uint MaxIndexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of vertex buffer slots.</summary>
        internal uint MaxVertexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of constant buffer slots.</summary>
        internal uint MaxConstantBufferSlots { get; private set; }

        /// <summary>Gets the maximum number of supported un-ordered access views in a compute shader.</summary>
        internal uint MaxUnorderedAccessViews { get; private set; }

        /// <summary>Gets whether or not the device supports hardware instances.</summary>
        internal bool HardwareInstancing { get; private set; }

        /// <summary>Gets whether or not concurrent resource creation is supported. If false, the driver cannot render while creating resources at the same time.
        /// Loading resources will stall rendering until creation is finished.</summary>
        internal bool ConcurrentResources { get; private set; }

        /// <summary>Gets whether or not command lists are supported. That is, rendering commands issued by an immediate context can be concurrent with 
        /// object creation on separate threads with low risk of a frame rate stutter.</summary>
        internal DX11CommandListSupport CommandListSupport { get; private set; }

        /// <summary>
        /// Gets the maximum number of render targets (draw buffers) a fragment shader can output to simultaneously.
        /// </summary>
        internal uint SimultaneousRenderSurfaces { get; private protected set; }
    }
}
