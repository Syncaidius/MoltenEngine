using System.Reflection;

namespace Molten.Graphics.DX11;
public class ResourceManagerDX11 : GraphicsResourceManager<DeviceDX11>
{
    FxcCompiler _shaderCompiler;

    internal ResourceManagerDX11(DeviceDX11 device) :
        base(device)
    {
        Assembly includeAssembly = GetType().Assembly;
        _shaderCompiler = new FxcCompiler(device, "\\Assets\\HLSL\\include\\", includeAssembly);
    }

    public override IDepthStencilSurface CreateDepthSurface(uint width, uint height,
        DepthFormat format = DepthFormat.R24G8,
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        MSAAQuality msaa = MSAAQuality.CenterPattern;
        return new DepthSurfaceDX11(Device, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, name);
    }

    protected override ShaderPass OnCreateShaderPass(Shader shader, string name = null)
    {
        return new ShaderPassDX11(shader, name);
    }

    protected override INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height,
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm, uint mipCount = 1)
    {
        return new FormSurfaceDX11(Device, width, height, 1, formTitle, formName);
    }

    protected override INativeSurface OnCreateControlSurface(string formTitle, string controlName, uint mipCount = 1)
    {
        throw new NotImplementedException(); // return new RenderControlSurface(this, formTitle, controlName, mipCount);
    }

    public override IRenderSurface2D CreateSurface(uint width, uint height,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        uint mipCount = 1,
        uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        MSAAQuality msaa = MSAAQuality.CenterPattern;
        return new RenderSurface2DDX11(Device, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, name);
    }

    public override ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize,
        GraphicsFormat format, GraphicsResourceFlags flags, string name = null)
    {
        return new Texture1DDX11(Device, width, flags, format, mipCount, arraySize, name);
    }

    public override ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize,
        GraphicsFormat format, GraphicsResourceFlags flags,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality aaQuality = MSAAQuality.Default, string name = null)
    {
        return new Texture2DDX11(Device, width, height, flags, format, mipCount, arraySize, aaLevel, aaQuality, name);
    }

    public override ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount, GraphicsFormat format, GraphicsResourceFlags flags, string name = null)
    {
        return new Texture3DDX11(Device, width, height, depth, flags, format, mipCount, name);
    }

    public override ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GraphicsFormat format,
        uint cubeCount = 1, uint arraySize = 1, GraphicsResourceFlags flags = GraphicsResourceFlags.None, string name = null)
    {
        return new TextureCubeDX11(Device, width, height, flags, format, mipCount, cubeCount, name);
    }

    /// <summary>
    /// Resolves a source texture into a destination texture. <para/>
    /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
    /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <param name="destination">The destination texture.</param>
    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination)
    {
        if (source.ResourceFormat != destination.ResourceFormat)
            throw new Exception("The source and destination texture must be the same format.");

        uint arrayLevels = Math.Min(source.ArraySize, destination.ArraySize);
        uint mipLevels = Math.Min(source.MipMapCount, destination.MipMapCount);

        for (uint i = 0; i < arrayLevels; i++)
        {
            for (uint j = 0; j < mipLevels; j++)
            {
                TextureResolve task = Device.Tasks.Get<TextureResolve>();
                task.Destination = destination as TextureDX11;
                task.SourceMipLevel = j;
                task.SourceArraySlice = i;
                task.DestMipLevel = j;
                task.DestArraySlice = i;
                Device.Tasks.Push(GraphicsPriority.StartOfFrame, source as TextureDX11, task);
            }
        }
    }

    /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
    /// <param name="source">The source texture.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="sourceMipLevel">The source mip-map level.</param>
    /// <param name="sourceArraySlice">The source array slice.</param>
    /// <param name="destMiplevel">The destination mip-map level.</param>
    /// <param name="destArraySlice">The destination array slice.</param>
    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination,
        uint sourceMipLevel,
        uint sourceArraySlice,
        uint destMiplevel,
        uint destArraySlice)
    {
        if (source.ResourceFormat != destination.ResourceFormat)
            throw new Exception("The source and destination texture must be the same format.");

        TextureResolve task = Device.Tasks.Get<TextureResolve>();
        task.Destination = destination as TextureDX11;
        Device.Tasks.Push(GraphicsPriority.StartOfFrame, source as TextureDX11, task);
    }

    protected override ShaderSampler OnCreateSampler(ShaderSamplerParameters parameters)
    {
        return new SamplerDX11(Device, parameters);
    }

    protected unsafe override GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format, uint numElements, T[] initialData)
    {
        uint stride = (uint)sizeof(T);
        uint initialBytes = initialData != null ? (uint)initialData.Length * stride : 0;

        fixed (T* ptrData = initialData)
            return new BufferDX11(Device, type, flags, format, stride, numElements, 1, ptrData, initialBytes);
    }

    public override IConstantBuffer CreateConstantBuffer(ConstantBufferInfo info)
    {
        return new ConstantBufferDX11(Device, info);
    }

    protected override void OnGraphicsRelease()
    {
        _shaderCompiler?.Dispose(true);
    }

    /// <inheritdoc/>
    public override FxcCompiler Compiler => _shaderCompiler;
}
