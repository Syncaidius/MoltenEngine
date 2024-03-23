using Molten.Graphics.Dxc;
using System.Reflection;

namespace Molten.Graphics.Vulkan;
public class ResourceManagerVK :GraphicsResourceManager<DeviceVK>
{
    SpirvCompiler _shaderCompiler;

    internal ResourceManagerVK(DeviceVK device) : 
        base(device)
    {
        Assembly includeAssembly = GetType().Assembly;
        _shaderCompiler = new SpirvCompiler(device, "\\Assets\\HLSL\\include\\", includeAssembly, SpirvCompileTarget.Vulkan1_1);
    }

    protected override ShaderPass OnCreateShaderPass(Shader shader, string name)
    {
        return new ShaderPassVK(shader, name);
    }

    /// <inheritdoc/>
    protected override ShaderSampler OnCreateSampler(ShaderSamplerParameters parameters)
    {
        throw new NotImplementedException();
    }

    protected unsafe override GpuBuffer CreateBuffer<T>(GpuBufferType type, GpuResourceFlags flags, GpuResourceFormat format, ulong numElements, T[] initialData = null)
    {
        BufferVK buffer = new BufferVK(Device, type, flags, (uint)sizeof(T), numElements, 1);

        if (initialData != null)
            buffer.SetData(GpuPriority.Apply, initialData, false);

        return buffer;
    }

    public override IConstantBuffer CreateConstantBuffer(ConstantBufferInfo info)
    {
        throw new NotImplementedException();
    }

    protected override INativeSurface OnCreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
    {
        throw new NotImplementedException();
    }

    public override IDepthStencilSurface CreateDepthSurface(uint width, uint height,
        DepthFormat format = DepthFormat.R24G8,
        GpuResourceFlags flags = GpuResourceFlags.DefaultMemory,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        return new DepthSurfaceVK(Device, width, height, mipCount, arraySize, aaLevel, MSAAQuality.Default, format, flags, name);
    }

    public override IRenderSurface2D CreateSurface(uint width, uint height,
        GpuResourceFormat format = GpuResourceFormat.R8G8B8A8_SNorm,
        GpuResourceFlags flags = GpuResourceFlags.DefaultMemory,
        uint mipCount = 1, uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        return new RenderSurface2DVK(Device, width, height, mipCount, arraySize, aaLevel, MSAAQuality.Default, format, flags, name);
    }

    protected override INativeSurface OnCreateFormSurface(
        string formTitle,
        string formName,
        uint width,
        uint height,
        GpuResourceFormat format = GpuResourceFormat.B8G8R8A8_UNorm,
        uint mipCount = 1)
    {
        return new WindowSurfaceVK(Device, formTitle, width, height, mipCount, GpuResourceFlags.None, format);
    }

    public override void ResolveTexture(GpuTexture source, GpuTexture destination)
    {
        throw new NotImplementedException();
    }

    public override void ResolveTexture(GpuTexture source, GpuTexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
    {
        throw new NotImplementedException();
    }

    public override ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize, GpuResourceFormat format, GpuResourceFlags flags, string name = null)
    {
        return new Texture1DVK(Device, width, mipCount, arraySize, format, flags, name);
    }

    public override ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize,
        GpuResourceFormat format,
        GpuResourceFlags flags,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality aaQuality = MSAAQuality.Default,
        string name = null)
    {
        return new Texture2DVK(Device, width, height, mipCount, arraySize,
            aaLevel, aaQuality,
            format,
            flags,
            name);
    }

    public override ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount,
        GpuResourceFormat format,
        GpuResourceFlags flags,
        string name = null)
    {
        TextureDimensions dim = new TextureDimensions(width, height, depth, mipCount, 1);
        return new Texture3DVK(Device, dim, format, flags, name);
    }
    public override ITextureCube CreateTextureCube(uint width, uint height, uint mipCount,
        GpuResourceFormat format, uint cubeCount = 1, uint arraySize = 1,
        GpuResourceFlags flags = GpuResourceFlags.None, string name = null)
    {
        return new TextureCubeVK(Device, width, height, mipCount, 1, cubeCount, format, flags, name);
    }

    protected override void OnGraphicsRelease()
    {
        _shaderCompiler?.Dispose();
    }

    /// <inheritdoc/>
    protected override DxcCompiler Compiler => _shaderCompiler;
}
