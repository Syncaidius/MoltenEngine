using Molten.Graphics.Dxc;
using Silk.NET.Direct3D12;
using System.Reflection;

namespace Molten.Graphics.DX12;
internal class ResourceManagerDX12 : GraphicsResourceManager<DeviceDX12>
{
    HlslDxcCompiler _shaderCompiler;

    public ResourceManagerDX12(DeviceDX12 device) : 
        base(device)
    {

        Assembly includeAssembly = GetType().Assembly;
        _shaderCompiler = new HlslDxcCompiler(device, "\\Assets\\HLSL\\include\\", includeAssembly);
    }

    protected override ShaderSampler OnCreateSampler(ShaderSamplerParameters parameters)
    {
        // TODO Add support for heap-based (non-static) samplers
        if (parameters.IsStatic)
            return new StaticSamplerDX12(Device, parameters);
        else
            throw new NotImplementedException("Heap-based (non-static) samplers are not implemented yet!");
    }

    protected override ShaderPass OnCreateShaderPass(Shader shader, string name)
    {
        return new ShaderPassDX12(shader, name);
    }

    protected unsafe override GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format, uint numElements, T[] initialData)
    {
        uint stride = (uint)sizeof(T);
        uint alignment = stride;
        if (type == GraphicsBufferType.Constant)
        {
            alignment = D3D12.ConstantBufferDataPlacementAlignment; // Constant buffers must be 256-bit aligned.

            if (stride % 256 != 0)
                throw new GraphicsStrideException(stride, $"The data type of a DX12 constant buffer must be a multiple of 256 bytes.");
        }

        BufferDX12 buffer = new BufferDX12(Device, stride, numElements, flags, type, alignment);
        if (initialData != null)
        {
            // TODO send initial data to the buffer.
            // buffer.SetData<T>(GraphicsPriority.Immediate, initialData);
        }

        return buffer;
    }

    public override IConstantBuffer CreateConstantBuffer(ConstantBufferInfo info)
    {
        return new ConstantBufferDX12(Device, info);
    }

    protected override INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height,
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm, uint mipCount = 1)
    {
        return new FormSurfaceDX12(Device, width, height, 1, formTitle, formName);
    }

    protected override INativeSurface OnCreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
    {
        throw new NotImplementedException();
    }

    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination)
    {
        throw new NotImplementedException();
    }

    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination, uint sourceMipLevel,
        uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
    {
        throw new NotImplementedException();
    }

    public override IRenderSurface2D CreateSurface(uint width, uint height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, uint mipCount = 1,
        uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        return new RenderSurface2DDX12(Device, width, height, flags, format, mipCount, arraySize, aaLevel, MSAAQuality.Default, name);
    }

    public override IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, uint mipCount = 1, uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        return new DepthSurfaceDX12(Device, width, height, flags, format, mipCount, arraySize, aaLevel, MSAAQuality.Default, name);
    }

    public override ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize, GraphicsFormat format, GraphicsResourceFlags flags, string name = null)
    {
        return new Texture1DDX12(Device, width, flags, format, mipCount, arraySize, name);
    }

    public override ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize, GraphicsFormat format,
        GraphicsResourceFlags flags, AntiAliasLevel aaLevel = AntiAliasLevel.None, MSAAQuality aaQuality = MSAAQuality.Default, string name = null)
    {
        return new Texture2DDX12(Device, width, height, flags, format, mipCount, arraySize, aaLevel, aaQuality, name);
    }

    public override ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount, GraphicsFormat format,
        GraphicsResourceFlags flags, string name = null)
    {
        return new Texture3DDX12(Device, width, height, depth, flags, format, mipCount, name);
    }

    public override ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GraphicsFormat format, uint cubeCount = 1, uint arraySize = 1,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None, string name = null)
    {
        return new TextureCubeDX12(Device, width, height, flags, format, mipCount, cubeCount, arraySize, name);
    }

    protected override void OnGraphicsRelease()
    {
        _shaderCompiler?.Dispose(true);
    }

    /// <inheritdoc/>
    protected override DxcCompiler Compiler => _shaderCompiler;
}
