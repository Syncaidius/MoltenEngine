using Molten.Collections;
using Molten.Graphics.Textures;
using Molten.IO;
using System.Reflection;

namespace Molten.Graphics;

public abstract class GpuResourceManager : GpuObject
{
    ThreadedList<ISwapChainSurface> _outputSurfaces;

    protected GpuResourceManager(GpuDevice device) : 
        base(device)
    {
        Renderer = Device.Renderer;
        _outputSurfaces = new ThreadedList<ISwapChainSurface>();
    }

    #region Shaders
    protected abstract ShaderPass OnCreateShaderPass(Shader shader, string name);

    internal ShaderPass CreateShaderPass(Shader shader, string name = null)
    {
        return OnCreateShaderPass(shader, name);
    }

    /// <summary>
    /// Loads an embedded shader from the target assembly. If an assembly is not provided, the current renderer's assembly is used instead.
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <param name="filename"></param>
    /// <param name="assembly">The assembly that contains the embedded shadr. If an assembly is not provided, the current renderer's assembly is used instead.</param>
    /// <returns></returns>
    public ShaderCompileResult LoadEmbeddedShader(string nameSpace, string filename, Assembly assembly = null)
    {
        string src = "";
        assembly ??= typeof(RenderService).Assembly;
        Stream stream = EmbeddedResource.TryGetStream($"{nameSpace}.{filename}", assembly);
        if (stream != null)
        {
            using (StreamReader reader = new StreamReader(stream))
                src = reader.ReadToEnd();

            stream.Dispose();
        }
        else
        {
            Device.Log.Error($"Attempt to load embedded shader failed: '{filename}' not found in namespace '{nameSpace}' of assembly '{assembly.FullName}'");
            return new ShaderCompileResult();
        }

        return Compiler.Compile(src, filename, ShaderCompileFlags.None, assembly, nameSpace);
    }

    /// <summary>Compiles a set of shaders from the provided source string.</summary>
    /// <param name="source">The source code to be parsed and compiled.</param>
    /// <param name="filename">The name of the source file. Used as a pouint of reference in debug/error messages only.</param>
    /// <returns></returns>
    public ShaderCompileResult CompileShaders(ref string source, string filename = null)
    {
        return Compiler.Compile(source, filename, ShaderCompileFlags.None, null, null);
    }
    #endregion

    #region Buffers

    public abstract IConstantBuffer CreateConstantBuffer(ConstantBufferInfo info);

    public GpuBuffer CreateBuffer<T>(GpuBufferType type, GpuResourceFlags flags, GpuResourceFormat format, 
        uint numElements, uint alignment = 1,
        T[] initialData = null)
        where T : unmanaged
    {
        SetBufferProperties(type, ref flags, ref format);
        GpuBuffer buffer = OnCreateBuffer<T>(type, flags, format, numElements, initialData);

        return buffer;
    }

    private void SetBufferProperties(GpuBufferType type, ref GpuResourceFlags flags, ref GpuResourceFormat format)
    {
        switch (type)
        {
            case GpuBufferType.Download:
                flags = GpuResourceFlags.DownloadMemory | GpuResourceFlags.DenyShaderAccess;
                format = GpuResourceFormat.Unknown;
                break;

            case GpuBufferType.Upload:
                flags = GpuResourceFlags.UploadMemory | GpuResourceFlags.DenyShaderAccess;
                format = GpuResourceFormat.Unknown;
                break;

            case GpuBufferType.Structured:
                format = GpuResourceFormat.Unknown;
                break;

            case GpuBufferType.Vertex:
                format = GpuResourceFormat.Unknown;
                flags |= GpuResourceFlags.DenyShaderAccess;
                break;

            case GpuBufferType.Index:
                if(format != GpuResourceFormat.R32_UInt && format != GpuResourceFormat.R16_UInt && format != GpuResourceFormat.R8_UInt)
                    throw new ArgumentException($"Invalid index buffer format '{format}'. Must be: R16_UInt, R32_UInt or R8_UInt");

                flags |= GpuResourceFlags.DenyShaderAccess;
                break;

            case GpuBufferType.Constant:
                throw new NotSupportedException($"Constant buffers must be created via the {nameof(CreateConstantBuffer)}() method.");
        }
    }

    protected abstract GpuBuffer OnCreateBuffer<T>(GpuBufferType type, GpuResourceFlags flags, GpuResourceFormat format,
        ulong numElements, T[] initialData = null) where T : unmanaged;
    #endregion

    #region Samplers
    /// <summary>
    /// Requests a new <see cref="ShaderSampler"/> from the current <see cref="GpuDevice"/>, with the implementation's default sampler settings.
    /// </summary>
    /// <param name="parameters">The parameters to use when creating the new <see cref="ShaderSampler"/>.</param>
    /// <returns></returns>
    public ShaderSampler CreateSampler(ShaderSamplerParameters parameters)
    {
        ShaderSampler sampler = OnCreateSampler(parameters);
        Device.Cache.Check(ref sampler);
        return sampler;
    }

    protected abstract ShaderSampler OnCreateSampler(ShaderSamplerParameters parameters);
    #endregion

    #region Meshes
    /// <summary>
    /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public Mesh<GBufferVertex> CreateMesh(GpuResourceFlags mode, ushort maxVertices, uint maxIndices, GBufferVertex[] initialVertices, ushort[] initialIndices)
    {
        return new StandardMesh(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
    }

    /// <summary>
    /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public Mesh<GBufferVertex> CreateMesh(GpuResourceFlags mode, uint maxVertices, uint maxIndices = 0, GBufferVertex[] initialVertices = null, uint[] initialIndices = null)
    {
        return new StandardMesh(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
    }

    public Mesh<T> CreateMesh<T>(T[] vertices, ushort[] indices, GpuResourceFlags flags = GpuResourceFlags.DefaultMemory)
        where T : unmanaged, IVertexType
    {
        if (vertices == null)
            throw new ArgumentNullException($"Vertices array cannot be nulled");

        if (vertices.Length >= ushort.MaxValue)
            throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

        uint indexCount = indices != null ? (uint)indices.Length : 0;
        return CreateMesh(flags, (ushort)vertices.Length, indexCount, vertices, indices);
    }

    public Mesh<T> CreateMesh<T>(T[] vertices, uint[] indices = null, GpuResourceFlags flags = GpuResourceFlags.DefaultMemory)
where T : unmanaged, IVertexType
    {
        if (vertices == null)
            throw new ArgumentNullException($"Vertices array cannot be nulled");

        uint indexCount = indices != null ? (uint)indices.Length : 0;
        return CreateMesh(flags, (uint)vertices.Length, indexCount, vertices, indices);
    }

    /// <summary>
    /// Creates a new mesh. Index data is optional, but can potentially lead to less data transfer when copying to/from the GPU.
    /// </summary>
    /// <typeparam name="T">The type of vertex data.</typeparam>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public Mesh<T> CreateMesh<T>(GpuResourceFlags mode, ushort maxVertices, uint maxIndices, T[] initialVertices, ushort[] initialIndices)
        where T : unmanaged, IVertexType
    {
        return new Mesh<T>(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
    }

    /// <summary>
    /// Creates a new mesh. Index data is optional, but can potentially lead to less data transfer when copying to/from the GPU.
    /// </summary>
    /// <typeparam name="T">The type of vertex data.</typeparam>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public Mesh<T> CreateMesh<T>(GpuResourceFlags mode, uint maxVertices, uint maxIndices = 0, T[] initialVertices = null, uint[] initialIndices = null)
        where T : unmanaged, IVertexType
    {
        return new Mesh<T>(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
    }

    public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, ushort[] indices, GpuResourceFlags flags = GpuResourceFlags.None)
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexInstanceType
    {
        if (vertices.Length >= ushort.MaxValue)
            throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

        uint maxIndices = indices != null ? (uint)indices.Length : 0;
        return new InstancedMesh<V, I>(Renderer, flags, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
    }

    public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, uint[] indices = null, GpuResourceFlags flags = GpuResourceFlags.None)
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexInstanceType
    {
        uint maxIndices = indices != null ? (uint)indices.Length : 0;
        return new InstancedMesh<V, I>(Renderer, flags, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
    }

    /// <summary>
    /// Creates a instanced mesh.
    /// </summary>
    /// <typeparam name="V">The type of vertex data.</typeparam>
    /// <typeparam name="I">The type if instance data.</typeparam>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxInstances"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public InstancedMesh<V, I> CreateInstancedMesh<V, I>(GpuResourceFlags mode, ushort maxVertices, uint maxInstances, uint maxIndices, V[] initialVertices, ushort[] initialIndices)
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexInstanceType
    {
        if (initialVertices != null && initialVertices.Length >= ushort.MaxValue)
            throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

        return new InstancedMesh<V, I>(Renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
    }

    /// <summary>
    /// Creates a instanced mesh.
    /// </summary>
    /// <typeparam name="V">The type of vertex data.</typeparam>
    /// <typeparam name="I">The type if instance data.</typeparam>
    /// <param name="mode"></param>
    /// <param name="maxVertices"></param>
    /// <param name="maxInstances"></param>
    /// <param name="maxIndices"></param>
    /// <param name="initialVertices"></param>
    /// <param name="initialIndices"></param>
    /// <returns></returns>
    public InstancedMesh<V, I> CreateInstancedMesh<V, I>(GpuResourceFlags mode, uint maxVertices, uint maxInstances, uint maxIndices = 0,
        V[] initialVertices = null, uint[] initialIndices = null)
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexInstanceType
    {
        return new InstancedMesh<V, I>(Renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
    }
    #endregion

    #region Textures
    public GpuTexture CreateUploadTexture(ITexture src)
    {
        GpuResourceFlags flags = GpuResourceFlags.UploadMemory | GpuResourceFlags.DenyShaderAccess;
        string name = src.Name + "_upload";
        return CreateTransferTexture(src, flags, name);
    }

    public GpuTexture CreateDownloadTexture(ITexture src)
    {
        GpuResourceFlags flags = GpuResourceFlags.DownloadMemory | GpuResourceFlags.DenyShaderAccess;
        string name = src.Name + "_download";
        return CreateTransferTexture(src, flags, name);
    }

    private GpuTexture CreateTransferTexture(ITexture src, GpuResourceFlags flags, string name)
    {
        ITexture result = src switch
        {
            ITexture1D => CreateTexture1D(src.Width, src.MipMapCount, src.ArraySize, src.ResourceFormat, flags, name),
            ITextureCube cube => CreateTextureCube(src.Width, src.Height, src.MipMapCount, src.ResourceFormat, cube.CubeCount, src.ArraySize, flags, name),
            ITexture2D => CreateTexture2D(src.Width, src.Height, src.MipMapCount, src.ArraySize, src.ResourceFormat, flags, src.MultiSampleLevel, src.SampleQuality, name),
            ITexture3D => CreateTexture3D(src.Width, src.Height, src.Depth, src.MipMapCount, src.ResourceFormat, flags, name),

            _ => throw new GpuResourceException(src as GpuTexture, "Unsupported staging texture type"),
        };

        return result as GpuTexture;
    }

    /// <summary>
    /// Creates a new 1D texture and returns it.
    /// </summary>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="mipCount">The number of mip-map levels.</param>
    /// <param name="arraySize">The number of array slices.</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/>.</param>
    /// <param name="flags">Resource creation flags.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    /// <returns></returns>
    /// <returns></returns>
    public abstract ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize,
        GpuResourceFormat format, GpuResourceFlags flags, string name = null);

    /// <summary>Creates a new 1D texture and returns it.</summary>
    /// <param name="data">The data from which to create the texture.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    public ITexture1D CreateTexture1D(TextureData data, string name = null)
    {
        ITexture1D tex = CreateTexture1D(data.Width, data.MipMapLevels, data.ArraySize, data.Format, data.Flags, name);
        tex.SetData(GpuPriority.StartOfFrame, data, 0, 0, data.MipMapLevels, data.ArraySize);
        return tex;
    }

    /// <summary>
    /// Creates a new 2D texture and returns it.
    /// </summary>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount">The number of mip-map levels.</param>
    /// <param name="arraySize">The number of array slices.</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/>.</param>
    /// <param name="flags">Resource creation flags.</param>
    /// <param name="aaLevel">The number of samples to perform for multi-sampled anti-aliasing (MSAA).</param>
    /// <param name="aaQuality">The quality preset to use for multi-sampled anti-aliasing (MSAA).</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    /// <returns></returns>
    public abstract ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize,
            GpuResourceFormat format,
            GpuResourceFlags flags,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality aaQuality = MSAAQuality.Default, string name = null);

    /// <summary>Creates a new 2D texture and returns it.</summary>
    /// <param name="data">The data from which to create the texture.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    public ITexture2D CreateTexture2D(TextureData data, string name = null)
    {
        ITexture2D tex = CreateTexture2D(data.Width, data.Height, data.MipMapLevels, data.ArraySize,
            data.Format,
            data.Flags,
            data.MultiSampleLevel,
            data.MultiSampleQuality,
            name);

        tex.SetData(GpuPriority.StartOfFrame, data, 0, 0, data.MipMapLevels, data.ArraySize);
        return tex;
    }

    /// <summary>
    /// Creates a new 3D texture and returns it.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="depth">The depth of the 3D texture.</param>
    /// <param name="mipCount">The number of mip-map levels.</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/>.</param>
    /// <param name="flags">Resource creation flags.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    /// <returns></returns>
    public abstract ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount,
                   GpuResourceFormat format, GpuResourceFlags flags, string name = null);

    /// <summary>Creates a new 3D texture and returns it.</summary>
    /// <param name="data">The data from which to create the texture.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    public ITexture3D CreateTexture3D(TextureData data, string name = null)
    {
        ITexture3D tex = CreateTexture3D(data.Width, data.Height, data.Depth, data.MipMapLevels, data.Format, data.Flags, name);
        tex.SetData(GpuPriority.StartOfFrame, data, 0, 0, data.MipMapLevels, data.ArraySize);
        return tex;
    }

    /// <summary>
    /// Creates a new cube texture (cube-map) and returns it.
    /// </summary>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount">The number of mip-map levels.</param>
    /// <param name="cubeCount">The number texture cubes (cube array slices).</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/>.</param>
    /// <param name="flags">Resource creation flags.</param>
    /// <param name="arraySize">The number of array slices.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    /// <returns></returns>
    public abstract ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GpuResourceFormat format,
        uint cubeCount = 1, uint arraySize = 1, GpuResourceFlags flags = GpuResourceFlags.None, string name = null);

    /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
    /// <param name="data">The data from which to create the texture.</param>
    /// <param name="cubeCount">The number of cubes to be represented in the texture. The <see cref="ITexture.ArraySize"/> will be <paramref name="cubeCount"/> * 6.</param>
    /// <param name="name">A custom name for the texture resource, if any.</param>
    public ITextureCube CreateTextureCube(TextureData data, uint cubeCount = 1, string name = null)
    {
        ITextureCube tex = CreateTextureCube(data.Width, data.Height, data.MipMapLevels, data.Format, cubeCount, data.ArraySize, data.Flags, name);
        tex.SetData(GpuPriority.StartOfFrame, data, 0, 0, data.MipMapLevels, data.ArraySize);
        return tex;
    }

    /// <summary>
    /// Resolves a source texture into a destination texture. <para/>
    /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
    /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <param name="destination">The destination texture.</param>
    public abstract void ResolveTexture(GpuTexture source, GpuTexture destination);

    /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
    /// <param name="source">The source texture.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="sourceMipLevel">The source mip-map level.</param>
    /// <param name="sourceArraySlice">The source array slice.</param>
    /// <param name="destMiplevel">The destination mip-map level.</param>
    /// <param name="destArraySlice">The destination array slice.</param>
    public abstract void ResolveTexture(GpuTexture source, GpuTexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice);
    #endregion

    #region Surfaces
    public abstract IRenderSurface2D CreateSurface(uint width, uint height, GpuResourceFormat format = GpuResourceFormat.R8G8B8A8_UNorm,
        GpuResourceFlags flags = GpuResourceFlags.DefaultMemory,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null);

    public abstract IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8,
        GpuResourceFlags flags = GpuResourceFlags.DefaultMemory,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null);

    /// <summary>Creates a form with a surface which can be rendered on to.</summary>
    /// <param name="formTitle">The title of the form.</param>
    /// <param name="formName">The internal name of the form.</param>
    /// <param name="width">The width of the form.</param>
    /// <param name="height">The height of the form.</param>
    /// <param name="format">The format of the form surface.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="enabled">Whether or not the form is enabled for presentation.</param>
    /// <returns></returns>
    public INativeSurface CreateFormSurface(string formTitle, string formName, uint width, uint height,
        GpuResourceFormat format = GpuResourceFormat.B8G8R8A8_UNorm,
        uint mipCount = 1, bool enabled = true)
    {
        INativeSurface surface = OnCreateFormSurface(formTitle, formName, width, height, format, mipCount);
        surface.IsEnabled = enabled;
        _outputSurfaces.Add(surface);
        return surface;
    }

    /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
    /// <param name="controlTitle">The title of the form.</param>
    /// <param name="controlName">The internal name of the control.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="enabled">Whether or not the control is enabled for presentation.</param>
    /// <returns></returns>
    public INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1, bool enabled = true)
    {
        INativeSurface surface = OnCreateControlSurface(controlTitle, controlName, mipCount);
        surface.IsEnabled = enabled;
        _outputSurfaces.Add(surface);
        return surface;
    }

    /// <summary>Creates a form with a surface which can be rendered on to.</summary>
    /// <param name="formTitle">The title of the form.</param>
    /// <param name="formName">The internal name of the form.</param>
    /// <param name="width">The width of the form.</param>
    /// <param name="height">The height of the form.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="format">The format of the form surface.</param>
    /// <returns></returns>
    protected abstract INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height,
        GpuResourceFormat format = GpuResourceFormat.B8G8R8A8_UNorm,
        uint mipCount = 1);

    /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
    /// <param name="controlTitle">The title of the form.</param>
    /// <param name="controlName">The internal name of the control.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <returns></returns>
    protected abstract INativeSurface OnCreateControlSurface(string controlTitle, string controlName, uint mipCount = 1);
    #endregion

    internal RenderService Renderer { get; }

    /// <summary>
    /// Gets a read-only list of registered output <see cref="ISwapChainSurface"/> objects.
    /// </summary>
    public IReadOnlyThreadedList<ISwapChainSurface> OutputSurfaces => _outputSurfaces;

    /// <summary>
    /// Gets the shader compiler bound to the current <see cref="GpuDevice"/>.
    /// </summary>
    protected abstract ShaderCompiler Compiler { get; }
}

public abstract class GraphicsResourceManager<T> : GpuResourceManager
    where T : GpuDevice
{
    protected GraphicsResourceManager(T device) : base(device)
    {
        Device = device;
    }

    public new T Device { get; }
}