using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public partial class GraphicsCapabilities
    {   
        Dictionary<ShaderType, ShaderStageCapabilities> _shaderCap;

        public GraphicsCapabilities()
        {
            VertexShader = new ShaderStageCapabilities();
            GeometryShader = new ShaderStageCapabilities();
            HullShader = new ShaderStageCapabilities();
            DomainShader = new ShaderStageCapabilities();
            PixelShader = new ShaderStageCapabilities();
            Compute = new ComputeCapabilities();

            _shaderCap = new Dictionary<ShaderType, ShaderStageCapabilities>()
            {
                [ShaderType.Vertex] = VertexShader,
                [ShaderType.Geometry] = GeometryShader,
                [ShaderType.Hull] = HullShader,
                [ShaderType.Domain] = DomainShader,
                [ShaderType.Pixel] = PixelShader,
                [ShaderType.Compute] = Compute
            };
        }

        /// <summary>
        /// Sets a shader property across all <see cref="ShaderStageCapabilities"/> in the current <see cref="GraphicsCapabilities"/> instance.
        /// <para>These include <see cref="VertexShader"/>, <see cref="GeometryShader"/>, <see cref="HullShader"/>, <see cref="DomainShader"/>, 
        /// <see cref="PixelShader"/> and <see cref="Compute"/>.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The name of the <see cref="ShaderStageCapabilities"/> property to be set.</param>
        /// <param name="value">The value to be set.</param>
        public void SetShaderCap<T>(string propertyName, T value)
            where T : unmanaged
        {
            Type sType = typeof(ShaderStageCapabilities);
            PropertyInfo pInfo = sType.GetProperty(propertyName);
            if(pInfo != null)
            {
                Type vType = typeof(T);
                if (pInfo.PropertyType.IsAssignableFrom(vType))
                {
                    foreach (ShaderStageCapabilities sCap in _shaderCap.Values)
                        pInfo.SetValue(sCap, value);
                }
            }
        }

        public bool HasCapabilities(GraphicsCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }

        public void LogIncompatibility(Logger log, GraphicsCapabilities other)
        {
            // TODO Logs any features that are missing in the current capabilities, compared to the 'other' capabilities.
        }

        /// <summary>
        /// Gets the capabilities of a shader stage based on the provided <see cref="ShaderType"/> value.
        /// </summary>
        /// <param name="type">The shader stage type.</param>
        /// <returns></returns>
        public ShaderStageCapabilities this[ShaderType type] => _shaderCap[type];

        /// <summary>
        /// Gets or sets the graphics API capability.
        /// </summary>
        public GraphicsApi Api { get; set; }

        /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 1D texture size of 1 x 2048.</summary>
        public uint MaxTexture1DSize { get; set; }

        /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 2D texture size of 2048 x 2048.</summary>
        public uint MaxTexture2DSize { get; set; }

        /// <summary>Gets the maximum size of a 3D texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTexture3DSize { get; set; }

        /// <summary>Gets the maximum size of a cube texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTextureCubeSize { get; set; }

        /// <summary>Gets whether or not texture cube arrays are supported.</summary>
        public bool TextureCubeArrays { get; set; }

        /// <summary>
        /// Gets the maximum number of array slices a texture array can have.
        /// </summary>
        public uint MaxTextureArraySlices { get; set; }

        /// <summary>
        /// Gets or sets the maximum supported shader model.
        /// </summary>
        public ShaderModel MaxShaderModel { get; set; }

        /// <summary>
        /// Gets vertex shader stage capabilities.
        /// </summary>
        public ShaderStageCapabilities VertexShader { get; }

        /// <summary>
        /// Gets geometry shader stage capabilities.
        /// </summary>
        public ShaderStageCapabilities GeometryShader { get; }

        /// <summary>
        /// Gets hull/control shader stage capabilities.
        /// </summary>
        public ShaderStageCapabilities HullShader { get; }

        /// <summary>
        /// Gets domain/evaluation shader stage capabilities.
        /// </summary>
        public ShaderStageCapabilities DomainShader { get; }

        /// <summary>
        /// Gets pixel/fragment shader stage capabilities.
        /// </summary>
        public ShaderStageCapabilities PixelShader { get; }

        /// <summary>
        /// Gets compute shader stage capabilities.
        /// </summary>
        public ComputeCapabilities Compute { get; }

        /// <summary>The maximum anisotropy level that the device supports. A level of 0 means that the device does not support anisotropic filtering.</summary>
        public float MaxAnisotropy { get; set; }

        public bool HardwareInstancing { get; set; }

        public bool OcclusionQueries { get; set; }

        /// <summary>
        /// Gets or sets whether non-power-of-two textures are supported.
        /// </summary>
        public bool NonPowerOfTwoTextures { get; set; }

        /// <summary>
        /// Gets or sets whether or not blend-state logic operations are supported.
        /// </summary>
        public bool BlendLogicOp { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of  samplers that can be included in a pipeline layout descriptor. Only relevant for DirectX 12 and Vulkan.
        /// </summary>
        public uint MaxDescriptorSamplers { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of samplers supported, per shader stage.
        /// </summary>
        public uint MaxShaderSamplers { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of sampler objects that are allowed to simultaneously exist on a device.
        /// </summary>
        public uint MaxAllocatedSamplers { get; set; }

        /// <summary>
        /// Gets the maximum number of primitives that can be drawn in a single draw call.
        /// </summary>
        public uint MaxPrimitiveCount { get; set; }

        /// <summary>
        /// Gets capabilities for vertex buffers.
        /// </summary>
        public VertexBufferCapabilities VertexBuffers { get; } = new VertexBufferCapabilities();

        /// <summary>
        /// Gets capabilities for constant/uniform buffers.
        /// </summary>
        public BufferCapabilities ConstantBuffers { get; } = new BufferCapabilities();

        /// <summary>
        /// Gets capabilities for unordered-access/storage buffers.
        /// </summary>
        public BufferCapabilities UnorderedAccessBuffers { get; } = new BufferCapabilities();

        /// <summary>
        /// Gets capabilities for structured/storage buffers.
        /// </summary>
        public BufferCapabilities StructuredBuffers { get; } = new BufferCapabilities();
    }
}
