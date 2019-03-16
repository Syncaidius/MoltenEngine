using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents an implementation of a renderer's resource manager.
    /// </summary>
    public interface IResourceManager : IDisposable
    {
        IRenderSurface CreateSurface(int width, int height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            int mipCount = 1, int arraySize = 1, int sampleCount = 1, TextureFlags flags = TextureFlags.None);

        IDepthStencilSurface CreateDepthSurface(int width, int height, DepthFormat format = DepthFormat.R24G8_Typeless, int mipCount = 1, int arraySize = 1, int sampleCount = 1,
            TextureFlags flags = TextureFlags.None);

        /// <summary>Creates a form with a surface which can be rendered on to.</summary>
        /// <param name="formTitle">The title of the form.</param>
        /// <param name="formName">The internal name of the form.</param>
        /// <param name="mipCount">The number of mip map levels of the form surface.</param>
        /// <param name="sampleCount">The number of samples. Anything greater than 1 will return a multi-sampled surface.</param>
        /// <returns></returns>
        INativeSurface CreateFormSurface(string formTitle, string formName, int mipCount = 1, int sampleCount = 1);

        /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
        /// <param name="controlTitle">The title of the form.</param>
        /// <param name="controlName">The internal name of the control.</param>
        /// <param name="mipCount">The number of mip map levels of the form surface.</param>
        /// <param name="sampleCount">The number of samples. Anything greater than 1 will return a multi-sampled surface.</param>
        /// <returns></returns>
        INativeSurface CreateControlSurface(string controlTitle, string controlName, int mipCount = 1, int sampleCount = 1);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="properties">A set of 1D texture properties.</param>
        ITexture CreateTexture1D(Texture1DProperties properties);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        ITexture CreateTexture1D(TextureData data);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        ITexture2D CreateTexture2D(Texture2DProperties properties);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        ITexture2D CreateTexture2D(TextureData data);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        ITextureCube CreateTextureCube(Texture2DProperties properties);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        ITextureCube CreateTextureCube(TextureData data);

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        void ResolveTexture(ITexture source, ITexture destination);

        /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="sourceMipLevel">The source mip-map level.</param>
        /// <param name="sourceArraySlice">The source array slice.</param>
        /// <param name="destMiplevel">The destination mip-map level.</param>
        /// <param name="destArraySlice">The destination array slice.</param>
        void ResolveTexture(ITexture source, ITexture destination, int sourceMipLevel, int sourceArraySlice, int destMiplevel, int destArraySlice);

        /// <summary>
        /// Creates a renderer for drawing sprites and primitives with a <see cref="SpriteBatcher"/> via the provided callback.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        ISpriteRenderer CreateSpriteRenderer(Action<SpriteBatcher> callback = null);

        /// <summary>
        /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
        /// </summary>
        /// <param name="maxVertices"></param>
        /// <param name="topology"></param>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        IMesh<GBufferVertex> CreateMesh(int maxVertices,
            VertexTopology topology = VertexTopology.TriangleList,
            bool dynamic = false);

        /// <summary>
        /// Creates the indexed mesh.
        /// </summary>
        /// <param name="dynamic">if set to <c>true</c> [dynamic].</param>
        /// <param name="dedicatedResource">if set to <c>true</c> [dedicated resource].</param>
        /// <returns></returns>
        IIndexedMesh<GBufferVertex> CreateIndexedMesh(int maxVertices, int maxIndices,
            VertexTopology topology = VertexTopology.TriangleList,
            bool dynamic = false);

        /// <summary>Creates a new unindexed mesh. Unindexed meshes do not contain an index buffer to reduce vertex data size.</summary>
        /// <param name="dynamic">if set to <c>true</c> [dynamic].</param>
        /// <param name="dedicatedResource">if set to <c>true</c>, the mesh is given its own dedicated resource buffer.</param>
        /// <returns></returns>
        IMesh<T> CreateMesh<T>(
            int maxVertices, 
            VertexTopology topology = VertexTopology.TriangleList,
            bool dynamic = false) 
            where T : struct, IVertexType;

        /// <summary>
        /// Creates the indexed mesh.
        /// </summary>
        /// <param name="dynamic">if set to <c>true</c> [dynamic].</param>
        /// <param name="dedicatedResource">if set to <c>true</c> [dedicated resource].</param>
        /// <returns></returns>
        IIndexedMesh<T> CreateIndexedMesh<T>(int maxVertices, int maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit,
            bool dynamic = false) 
            where T : struct, IVertexType;

        /// <summary>Compiles a set of shaders from the provided source string.</summary>
        /// <param name="source">The source code to be parsed and compiled.</param>
        /// <param name="filename">The name of the source file. Used as a point of reference in debug/error messages only.</param>
        /// <returns></returns>
        ShaderCompileResult CompileShaders(string source, string filename = null);
    }
}
