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

    public class GraphicsDeviceFeatures
    {
        Device _d3d;

        bool _concurrentResources;
        bool _commandLists;

        FeatureLevel _featureLevel;
        CounterCapabilities _counterCap;

        // DX11 resource limits: https://msdn.microsoft.com/en-us/library/windows/desktop/ff819065%28v=vs.85%29.aspx

        internal GraphicsDeviceFeatures(Device d3dDevice)
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
                    MaxPrimitiveCount = (int)Math.Pow(2, 32);
                    MaxInputResourceSlots = 128;
                    MaxInputSamplerSlots = 16;
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

            _d3d.CheckThreadingSupport(out _concurrentResources, out _commandLists);
        }

        /// <summary>Returns a set of format support flags for the specified format, or none if format support checks are not supported.</summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public FormatSupport GetSupportedFormatFeatures(Format format)
        {
            return _d3d.CheckFormatSupport(format);
        }

        /// <summary>Returns true if core features of the provided format are supported by the graphics device.</summary>
        /// <param name="format">The format to check for support on the device.</param>
        /// <returns></returns>
        public bool IsFormatSupported(Format format)
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
        public int GetMultisampleQualityLevels(Format format, int sampleCount)
        {
            return _d3d.CheckMultisampleQualityLevels(format, sampleCount);
        }

        /// <summary>Gets a <see cref="CounterCapabilities>"/> containing details of the device's counter support.</summary>
        public CounterCapabilities CounterSupport => _counterCap;

        /// <summary>Gets whether or not concurrent resource creation is supported. If false, the driver cannot render while creating resources at the same time.
        /// Loading resources will stall rendering until creation is finished.</summary>
        public bool ConcurrentResources => _concurrentResources;

        /// <summary>Gets whether or not command lists are supported. That is, rendering commands issued by an immediate context can be concurrent with 
        /// object creation on separate threads with low risk of a frame rate stutter.</summary>
        public bool CommandLists => _commandLists;

        /// <summary>Gets the feature level of the current device.</summary>
        public FeatureLevel Level { get; private set; }

        /// <summary>Gets ther number of render targets that can be drawn to at the same time.</summary>
        public int SimultaneousRenderSurfaces { get; private set; }

        /// <summary>Gets the maximum size of a single texture dimension i.e 2048 would mean the max texture size is 2048x2048.</summary>
        public int MaxTextureDimension { get; private set; }

        /// <summary>Gets the maximum size of a single cube map dimension i.e 2048 would mean the max map size is 2048x2048.</summary>
        public int MaxCubeMapDimension { get; private set; }

        /// <summary>Undocumented.</summary>
        public int MaxVolumeExtent { get; private set; }

        /// <summary>The maximum number of times a texture is allowed to repeat.</summary>
        public int MaxTextureRepeat { get; private set; }

        /// <summary>The maximum anisotropy level that the device supports.</summary>
        public int MaxAnisotropy { get; private set; }

        /// <summary>Gets the maximum number of primitives (triangles) the device can render in a single draw call.</summary>
        public int MaxPrimitiveCount { get; private set; }

        /// <summary>Gets the max number of shader resource input slots that the device supports for all shader stages.</summary>
        public int MaxInputResourceSlots { get; private set; }

        public int MaxInputSamplerSlots { get; private set; }

        /// <summary>Gets whether or not the device supports occlusion queries.</summary>
        public bool OcclusionQueries { get; private set; }

        /// <summary>Gets whether or not texture arrays are supported.</summary>
        public bool TextureArrays { get; private set; }

        /// <summary>Gets whether or not cube map arrays are supported. </summary>
        public bool CubeMapArrays { get; private set; }

        /// <summary>Gets whether or not non-power of 2 textures are supported.</summary>
        public bool NonPowerOfTwoTextures { get; private set; }

        /// <summary>Gets the maximum shader model supported by the graphics device.</summary>
        public ShaderModel MaximumShaderModel { get; private set; }

        /// <summary>Gets the maximum supported number of index buffer slots.</summary>
        public int MaxIndexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of vertex buffer slots.</summary>
        public int MaxVertexBufferSlots { get; private set; }

        /// <summary>Gets the maximum supported number of constant buffer slots.</summary>
        public int MaxConstantBufferSlots { get; private set; }

        /// <summary>Gets the maximum number of supported un-ordered access views in a compute shader.</summary>
        public int MaxUnorderedAccessViews { get; private set; }

        /// <summary>Gets whether or not the device supports hardware instances.</summary>
        public bool HardwareInstancing { get; private set; }

        /// <summary>Gets an instance of <see cref="GraphicsComputeFeatures"/> which contains the supported compute features of a <see cref="GraphicsDevice"/>.</summary>
        public GraphicsComputeFeatures Compute { get; private set; }

        public GraphicsShaderFeatures Shaders { get; private set; }

        public FeatureDataD3D11Options MiscFeatures { get; private set; }
    }
}
