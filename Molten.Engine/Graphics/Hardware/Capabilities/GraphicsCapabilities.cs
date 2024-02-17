using Silk.NET.Core;
using System.Reflection;

namespace Molten.Graphics;

public sealed partial class GraphicsCapabilities
{   
    Dictionary<ShaderType, ShaderStageCapabilities> _shaderCap;

    public GraphicsCapabilities()
    {
        VertexShader = new ShaderStageCapabilities();
        GeometryShader = new ShaderStageCapabilities();
        HullShader = new ShaderStageCapabilities();
        DomainShader = new ShaderStageCapabilities();
        PixelShader = new ShaderStageCapabilities();
        AmplificationShader = new ShaderStageCapabilities();
        MeshShader = new MeshShaderCapabilities();
        Compute = new ComputeCapabilities();

        _shaderCap = new Dictionary<ShaderType, ShaderStageCapabilities>()
        {
            [ShaderType.Vertex] = VertexShader,
            [ShaderType.Geometry] = GeometryShader,
            [ShaderType.Hull] = HullShader,
            [ShaderType.Domain] = DomainShader,
            [ShaderType.Pixel] = PixelShader,
            [ShaderType.Compute] = Compute,
            [ShaderType.Amplification] = AmplificationShader,
            [ShaderType.Mesh] = MeshShader,
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
        if (pInfo != null)
        {
            Type vType = value.GetType();
            if (value is Bool32 bValue && pInfo.PropertyType == typeof(bool))
            {
                foreach (ShaderStageCapabilities sCap in _shaderCap.Values)
                    pInfo.SetValue(sCap, bValue.Value == 1);
            }
            else if (pInfo.PropertyType.IsAssignableFrom(vType))
            {
                foreach (ShaderStageCapabilities sCap in _shaderCap.Values)
                    pInfo.SetValue(sCap, value);
            }
        }
    }

    public void AddShaderCap(ShaderType type, ShaderCapFlags flag)
    {
        if (_shaderCap.TryGetValue(type, out ShaderStageCapabilities cap))
            cap.Flags |= flag | ShaderCapFlags.IsSupported;
    }

    public void AddShaderCap(ShaderCapFlags flag)
    {
        foreach (ShaderStageCapabilities cap in _shaderCap.Values)
            cap.Flags |= flag | ShaderCapFlags.IsSupported;
    }

    public bool IsCompatible(GraphicsCapabilities other)
    {
        // TODO compare current to other. Current must have at least everything 'other' specifies.

        return true;
    }

    public void LogIncompatibility(Logger log, GraphicsCapabilities other)
    {
        // TODO Logs any features that are missing in the current capabilities, compared to the 'other' capabilities.
    }

    /// <summary>
    /// Checks all available <see cref="CommandSets"/> to see if any support the requested <see cref="CommandSetCapabilityFlags"/>.
    /// </summary>
    /// <param name="flags">The capability flags to check for.</param>
    /// <returns></returns>
    public bool HasCommandSet(CommandSetCapabilityFlags flags)
    {
        foreach(SupportedCommandSet set in CommandSets)
        {
            if ((set.CapabilityFlags & flags) == flags)
                return true;
        }

        return false;
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

    /// <summary>
    /// Gets a list of supported command sets.
    /// </summary>
    public List<SupportedCommandSet> CommandSets { get; } = new List<SupportedCommandSet>();

    /// <summary>
    /// Gets or sets whether deferred/multi-threaded command lists is supported/required.
    /// </summary>
    public CommandListSupport DeferredCommandLists { get; set; } = CommandListSupport.None;

    /// <summary>
    /// Gets or sets the graphics API capability.
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 1D texture size of 1 x 2048.</summary>
    public uint MaxTexture1DSize { get; set; }

    /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 2D texture size of 2048 x 2048.</summary>
    public uint MaxTexture2DSize { get; set; }

    /// <summary>Gets the maximum size of a 3D texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
    public uint MaxTexture3DSize { get; set; }

    /// <summary>Gets the maximum size of a cube texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
    public uint MaxTextureCubeSize { get; set; }

    /// <summary>
    /// Gets the maximum number of array slices a texture array can have.
    /// </summary>
    public uint MaxTextureArraySlices { get; set; }

    /// <summary>
    /// Gets or sets conservative rasterization feature support.
    /// </summary>
    public ConservativeRasterizationFlags ConservativeRasterization { get; set; }

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
    /// Gets the amplification/task shader stage capabilities.
    /// </summary>
    public ShaderStageCapabilities AmplificationShader { get; }

    /// <summary>
    /// Gets the mesh sahder stage capabilities.
    /// </summary>
    public MeshShaderCapabilities MeshShader { get; }

    /// <summary>
    /// Gets compute shader stage capabilities.
    /// </summary>
    public ComputeCapabilities Compute { get; }

    /// <summary>The maximum anisotropy level that the device supports. A level of 0 means that the device does not support anisotropic filtering.</summary>
    public float MaxAnisotropy { get; set; }

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

    /// <summary>Gets or sets the amount of dedicated video memory (VRAM), in megabytes. This is generally found in discrete/dedicated display adapters.</summary>
    public double DedicatedVideoMemory { get; set; }

    /// <summary>Gets or sets the amount of system memory (RAM) dedicated to the adapter, in megabytes. This is often found in integrated display adapters.</summary>
    public double DedicatedSystemMemory { get; set; }

    /// <summary>Gets or sets the amount of video memory (VRAM) that is being shared with the system, in megabytes. 
    /// <para>This area of memory is generally available for direct mapping by the local machine CPU.</para></summary>
    public double SharedVideoMemory { get; set; }

    /// <summary>Gets or sets the maximum amount of system memory (RAM) that can be shared with the the adapter.</summary>
    public double SharedSystemMemory { get; set; }

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

    /// <summary>
    /// Gets or sets the capabilities of the graphics device.
    /// </summary>
    public GraphicsCapFlags Flags { get; set; }
}
