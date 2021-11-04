using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D;

namespace Molten.Graphics
{
    using Device = SharpDX.Direct3D11.Device;

    internal class GraphicsDX11Features : GraphicsDeviceFeatures
    {
        Device _d3d;

        FeatureLevel _featureLevel;
        CounterCapabilities _counterCap;

        // DX11 resource limits: https://msdn.microsoft.com/en-us/library/windows/desktop/ff819065%28v=vs.85%29.aspx

        internal GraphicsDX11Features(Device d3dDevice)
        {
            _d3d = d3dDevice;
            _featureLevel = _d3d.FeatureLevel;

            Compute = new GraphicsComputeFeatures(_d3d);
            Shaders = new GraphicsShaderFeatures(_d3d, _featureLevel);
            MiscFeatures = _d3d.CheckD3D11Feature();
            _counterCap = _d3d.GetCounterCapabilities();

            //calculate level-specific features.
            switch (_d3d.FeatureLevel)
            {
                case FeatureLevel.Level_9_1:
                case FeatureLevel.Level_9_2:
                case FeatureLevel.Level_9_3:
                case FeatureLevel.Level_10_0:
                case FeatureLevel.Level_10_1:
                    throw new UnsupportedFeatureException("Feature level " + _d3d.FeatureLevel);

                case FeatureLevel.Level_11_0:
                    SimultaneousRenderSurfaces = 8;
                    MaxTextureDimension = 16384;
                    MaxCubeMapDimension = 16384;
                    MaxVolumeExtent = 2048;
                    MaxTextureRepeat = 16384;
                    MaxAnisotropy = 16;
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

                    Compute.MaxThreadGroupSize = 1024;
                    Compute.MaxThreadGroupZ = 64;
                    Compute.MaxDispatchXYDimension = 65535;
                    Compute.MaxDispatchZDimension = 65535;
                    Compute.Supported = _d3d.CheckFeatureSupport(SharpDX.Direct3D11.Feature.ComputeShaders);
                    
                    break;
            }

            if (_d3d.CheckThreadingSupport(out bool cResources, out bool cLists).Success)
            {
                ConcurrentResources = cResources;
                CommandLists = cLists;
            }
        }

        /// <summary>Returns a set of format support flags for the specified format, or none if format support checks are not supported.</summary>
        /// <param name="format"></param>
        /// <returns></returns>
        internal FormatSupport GetSupportedFormatFeatures(Format format)
        {
            return _d3d.CheckFormatSupport(format);
        }

        /// <summary>Returns true if core features of the provided format are supported by the graphics device.</summary>
        /// <param name="format">The format to check for support on the device.</param>
        /// <returns></returns>
        internal bool IsFormatSupported(Format format)
        {
            FormatSupport support = _d3d.CheckFormatSupport(format);

            bool supported = (support & FormatSupport.Texture2D) == FormatSupport.Texture2D;
            supported = supported && (support & FormatSupport.Texture3D) == FormatSupport.Texture3D;
            supported = supported && (support & FormatSupport.CpuLockable) == FormatSupport.CpuLockable;
            supported = supported && (support & FormatSupport.ShaderSample) == FormatSupport.ShaderSample;
            supported = supported && (support & FormatSupport.ShaderLoad) == FormatSupport.ShaderLoad;
            supported = supported && (support & FormatSupport.Mip) == FormatSupport.Mip;

            return supported;
        }



        /// <summary>Returns the number of supported quality levels for the specified format and sample count.</summary>
        /// <param name="format">The format to test quality levels against.</param>
        /// <param name="sampleCount">The sample count to test against.</param>
        /// <returns></returns>
        internal int GetMultisampleQualityLevels(Format format, int sampleCount)
        {
            return _d3d.CheckMultisampleQualityLevels(format, sampleCount);
        }

        /// <summary>Gets a <see cref="CounterCapabilities>"/> containing details of the device's counter support.</summary>
        internal CounterCapabilities CounterSupport => _counterCap;

        /// <summary>Gets the feature level of the current device.</summary>
        internal FeatureLevel Level { get; private set; }

        /// <summary>Gets an instance of <see cref="GraphicsComputeFeatures"/> which contains the supported compute features of a <see cref="DeviceDX11"/>.</summary>
        internal GraphicsComputeFeatures Compute { get; private set; }

        internal GraphicsShaderFeatures Shaders { get; private set; }

        internal FeatureDataD3D11Options MiscFeatures { get; private set; }

        /// <summary>Undocumented.</summary>
        internal int MaxVolumeExtent { get; private set; }

        /// <summary>The maximum number of times a texture is allowed to repeat.</summary>
        internal int MaxTextureRepeat { get; private set; }

        /// <summary>The maximum anisotropy level that the device supports.</summary>
        internal int MaxAnisotropy { get; private set; }

        /// <summary>Gets the maximum number of primitives (triangles) the device can render in a single draw call.</summary>
        internal uint MaxPrimitiveCount { get; private set; }

        /// <summary>Gets the max number of shader resource input slots that the device supports for all shader stages.</summary>
        internal int MaxInputResourceSlots { get; private set; }

        /// <summary>Gets the max number of sampler slots that the device supports for all shader stages.</summary>
        internal int MaxSamplerSlots { get; private set; }

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
        internal int MaxIndexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of vertex buffer slots.</summary>
        internal int MaxVertexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of constant buffer slots.</summary>
        internal int MaxConstantBufferSlots { get; private set; }

        /// <summary>Gets the maximum number of supported un-ordered access views in a compute shader.</summary>
        internal int MaxUnorderedAccessViews { get; private set; }

        /// <summary>Gets whether or not the device supports hardware instances.</summary>
        internal bool HardwareInstancing { get; private set; }

        /// <summary>Gets whether or not concurrent resource creation is supported. If false, the driver cannot render while creating resources at the same time.
        /// Loading resources will stall rendering until creation is finished.</summary>
        internal bool ConcurrentResources { get; private set; }

        /// <summary>Gets whether or not command lists are supported. That is, rendering commands issued by an immediate context can be concurrent with 
        /// object creation on separate threads with low risk of a frame rate stutter.</summary>
        internal bool CommandLists { get; private set; }
    }
}
