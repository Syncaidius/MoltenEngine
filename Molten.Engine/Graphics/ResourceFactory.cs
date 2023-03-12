using System.Reflection;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents an implementation of a renderer's resource factory.
    /// </summary>
    public abstract class ResourceFactory : EngineObject
    {
        RenderService _renderer;
        ShaderCompiler _compiler;

        public ResourceFactory(RenderService renderer, ShaderCompiler sCompiler)
        {
            _renderer = renderer;
            _compiler = sCompiler;
        }

        public IRenderSurface2D CreateSurface(Texture2DProperties properties)
        {
            return CreateSurface(properties.Width,
                properties.Height,
                properties.Format,
                properties.MipMapLevels,
                properties.ArraySize,
                properties.MultiSampleLevel,
                properties.Flags);
        }

        public abstract IRenderSurface2D CreateSurface(uint width, uint height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, TextureFlags flags = TextureFlags.None, string name = null);

        public abstract IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None,
            TextureFlags flags = TextureFlags.None, string name = null);

        /// <summary>Creates a form with a surface which can be rendered on to.</summary>
        /// <param name="formTitle">The title of the form.</param>
        /// <param name="formName">The internal name of the form.</param>
        /// <param name="mipCount">The number of mip map levels of the form surface.</param>
        /// <returns></returns>
        public abstract INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1);

        /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
        /// <param name="controlTitle">The title of the form.</param>
        /// <param name="controlName">The internal name of the control.</param>
        /// <param name="mipCount">The number of mip map levels of the form surface.</param>
        /// <returns></returns>
        public abstract INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="properties">A set of 1D texture properties.</param>
        public abstract ITexture CreateTexture1D(Texture1DProperties properties);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture CreateTexture1D(TextureData data);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        public abstract ITexture2D CreateTexture2D(Texture2DProperties properties);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture2D CreateTexture2D(TextureData data);

        /// <summary>Creates a new 3D texture and returns it.</summary>
        /// <param name="properties">A set of 3D texture properties.</param>
        public abstract ITexture3D CreateTexture3D(Texture3DProperties properties);

        /// <summary>Creates a new 3D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture3D CreateTexture3D(TextureData data);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        public abstract ITextureCube CreateTextureCube(Texture2DProperties properties);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITextureCube CreateTextureCube(TextureData data);

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        public abstract void ResolveTexture(ITexture source, ITexture destination);

        /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="sourceMipLevel">The source mip-map level.</param>
        /// <param name="sourceArraySlice">The source array slice.</param>
        /// <param name="destMiplevel">The destination mip-map level.</param>
        /// <param name="destArraySlice">The destination array slice.</param>
        public abstract void ResolveTexture(ITexture source, ITexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice);

        /// <summary>
        /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public Mesh<GBufferVertex> CreateMesh(BufferMode mode, ushort maxVertices, uint maxIndices, GBufferVertex[] initialVertices, ushort[] initialIndices)
        {
            return new StandardMesh(_renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
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
        public Mesh<GBufferVertex> CreateMesh(BufferMode mode, uint maxVertices, uint maxIndices = 0, GBufferVertex[] initialVertices = null, uint[] initialIndices = null)
        {
            return new StandardMesh(_renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        public Mesh<T> CreateMesh<T>(T[] vertices, ushort[] indices, BufferMode mode = BufferMode.Immutable)
            where T : unmanaged, IVertexType
        {
            if (vertices == null)
                throw new ArgumentNullException($"Vertices array cannot be nulled");

            if (vertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            uint indexCount = indices != null ? (uint)indices.Length : 0;
            return CreateMesh(mode, (ushort)vertices.Length, indexCount, vertices, indices);
        }

        public Mesh<T> CreateMesh<T>(T[] vertices, uint[] indices = null, BufferMode mode = BufferMode.Immutable)
    where T : unmanaged, IVertexType
        {
            if (vertices == null)
                throw new ArgumentNullException($"Vertices array cannot be nulled");

            uint indexCount = indices != null ? (uint)indices.Length : 0;
            return CreateMesh(mode, (uint)vertices.Length, indexCount, vertices, indices);
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
        public Mesh<T> CreateMesh<T>(BufferMode mode, ushort maxVertices, uint maxIndices, T[] initialVertices, ushort[] initialIndices)
            where T : unmanaged, IVertexType
        {
            return new Mesh<T>(_renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
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
        public Mesh<T> CreateMesh<T>(BufferMode mode, uint maxVertices, uint maxIndices = 0, T[] initialVertices = null, uint[] initialIndices = null)
            where T : unmanaged, IVertexType
        {
            return new Mesh<T>(_renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, ushort[] indices, BufferMode mode = BufferMode.Immutable)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            if (vertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            uint maxIndices = indices != null ? (uint)indices.Length : 0;
            return new InstancedMesh<V, I>(_renderer, mode, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
        }

        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, uint[] indices = null, BufferMode mode = BufferMode.Immutable)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            uint maxIndices = indices != null ? (uint)indices.Length : 0;
            return new InstancedMesh<V, I>(_renderer, mode, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
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
        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(BufferMode mode, ushort maxVertices, uint maxInstances, uint maxIndices, V[] initialVertices, ushort[] initialIndices)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            if (initialVertices != null && initialVertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            return new InstancedMesh<V, I>(_renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
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
        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(BufferMode mode, uint maxVertices, uint maxInstances, uint maxIndices = 0,
            V[] initialVertices = null, uint[] initialIndices = null)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            return new InstancedMesh<V, I>(_renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
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
            assembly = assembly ?? typeof(RenderService).Assembly;
            Stream stream = EmbeddedResource.TryGetStream($"{nameSpace}.{filename}", assembly);
            if(stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                    src = reader.ReadToEnd();

                stream.Dispose();
            }
            else
            {
                _renderer.Log.Error($"Attempt to load embedded shader failed: '{filename}' not found in namespace '{nameSpace}' of assembly '{assembly.FullName}'");
                return new ShaderCompileResult();
            }

            return _compiler.Compile(src, filename, ShaderCompileFlags.EmbeddedFile, assembly, nameSpace);
        }

        /// <summary>Compiles a set of shaders from the provided source string.</summary>
        /// <param name="source">The source code to be parsed and compiled.</param>
        /// <param name="filename">The name of the source file. Used as a pouint of reference in debug/error messages only.</param>
        /// <returns></returns>
        public ShaderCompileResult CompileShaders(ref string source, string filename = null)
        {
            ShaderCompileFlags flags = ShaderCompileFlags.EmbeddedFile;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                FileInfo fInfo = new FileInfo(filename);
                DirectoryInfo dir = fInfo.Directory;
                flags = ShaderCompileFlags.None;
            }

            return _compiler.Compile(source, filename, flags, null, null);
        }
    }
}
