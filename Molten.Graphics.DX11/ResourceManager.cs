using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>DirectX 11 implementation of <see cref="IResourceManager"/>.</summary>
    public class ResourceManager : IResourceManager
    {
        RendererDX11 _renderer;
        List<SpriteFont> _fontTable;

        internal ResourceManager(RendererDX11 renderer)
        {
            _renderer = renderer;
            _fontTable = new List<SpriteFont>();
        }

        public IDepthStencilSurface CreateDepthSurface(
            uint width,
            uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1,
            uint arraySize = 1,
            uint sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
        {
            return new DepthStencilSurface(_renderer, width, height, format, mipCount, arraySize, sampleCount, flags);
        }

        public INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1, uint sampleCount = 1)
        {
            return new RenderFormSurface(formTitle, formName, _renderer, mipCount);
        }

        public INativeSurface CreateControlSurface(string formTitle, string controlName, uint mipCount = 1, uint sampleCount = 1)
        {
            return new RenderControlSurface(formTitle, controlName, _renderer, mipCount);
        }

        public IRenderSurface CreateSurface(
            uint width,
            uint height,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            uint sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
        {
            return new RenderSurface(_renderer, width, height, (Format)format, mipCount, arraySize, sampleCount, flags);
        }

        public ITexture CreateTexture1D(Texture1DProperties properties)
        {
            return new Texture1DDX11(_renderer, properties.Width, properties.Format.ToApi(), properties.MipMapLevels, properties.ArraySize, properties.Flags);
        }

        public ITexture CreateTexture1D(TextureData data)
        {
            Texture1DDX11 tex = new Texture1DDX11(_renderer, data.Width, data.Format.ToApi(), data.MipMapLevels, data.ArraySize, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            return new Texture2DDX11(_renderer,
                properties.Width,
                properties.Height,
                properties.Format.ToApi(),
                properties.MipMapLevels,
                properties.ArraySize,
                properties.Flags,
                properties.SampleCount);
        }

        public ITexture2D CreateTexture2D(TextureData data)
        {
            Texture2DDX11 tex = new Texture2DDX11(_renderer,
                data.Width,
                data.Height,
                data.Format.ToApi(),
                data.MipMapLevels,
                data.ArraySize,
                data.Flags,
                data.SampleCount);

            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            uint cubeCount = Math.Max(properties.ArraySize / 6, 1);
            return new TextureCubeDX11(_renderer, properties.Width, properties.Height, properties.Format.ToApi(), properties.MipMapLevels, cubeCount, properties.Flags);
        }

        public ITextureCube CreateTextureCube(TextureData data)
        {
            uint cubeCount = Math.Max(data.ArraySize / 6, 1);
            TextureCubeDX11 tex = new TextureCubeDX11(_renderer, data.Width, data.Height, data.Format.ToApi(), data.MipMapLevels, cubeCount, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        public void ResolveTexture(ITexture source, ITexture destination)
        {
            if (source.DataFormat != destination.DataFormat)
                throw new Exception("The source and destination texture must be the same format.");

            uint arrayLevels = Math.Min(source.ArraySize, destination.ArraySize);
            uint mipLevels = Math.Min(source.MipMapCount, destination.MipMapCount);

            for (uint i = 0; i < arrayLevels; i++)
            {
                for (uint j = 0; j < mipLevels; j++)
                {
                    TextureResolve task = TextureResolve.Get();
                    task.Source = source as TextureBase;
                    task.Destination = destination as TextureBase;
                    task.SourceMipLevel = j;
                    task.SourceArraySlice = i;
                    task.DestMipLevel = j;
                    task.DestArraySlice = i;
                    _renderer.PushTask(task);
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
        public void ResolveTexture(ITexture source, ITexture destination,
            uint sourceMipLevel,
            uint sourceArraySlice,
            uint destMiplevel,
            uint destArraySlice)
        {
            if (source.DataFormat != destination.DataFormat)
                throw new Exception("The source and destination texture must be the same format.");

            TextureResolve task = TextureResolve.Get();
            task.Source = source as TextureBase;
            task.Destination = destination as TextureBase;
            _renderer.PushTask(task);
        }

        public void Dispose()
        {
            for (int i = 0; i < _fontTable.Count; i++)
                _fontTable[i].Dispose();

            _fontTable.Clear();
        }

        public ISpriteRenderer CreateSpriteRenderer(Action<SpriteBatcher> callback = null)
        {
            return new SpriteRendererDX11(_renderer.Device, callback);
        }

        IMesh<GBufferVertex> IResourceManager.CreateMesh(uint maxVertices, VertexTopology topology, bool dynamic)
        {
            return new StandardMesh(_renderer, (uint)maxVertices, topology, dynamic);
        }

        public IIndexedMesh<GBufferVertex> CreateIndexedMesh(uint maxVertices,
            uint maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            bool dynamic = false)
        {
            return new StandardIndexedMesh(_renderer, (uint)maxVertices, (uint)maxIndices, topology, IndexBufferFormat.Unsigned32Bit, dynamic);
        }

        public IMesh<T> CreateMesh<T>(uint maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false) 
            where T : struct, IVertexType
        {
            return new Mesh<T>(_renderer, maxVertices, topology, dynamic);
        }

        public IIndexedMesh<T> CreateIndexedMesh<T>(
            uint maxVertices, 
            uint maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit, 
            bool dynamic = false)
            where T : struct, IVertexType
        {
            return new IndexedMesh<T>(_renderer, (uint)maxVertices, (uint)maxIndices, topology, indexFormat, dynamic);
        }

        /// <summary>Compiels a set of shaders from the provided source string.</summary>
        /// <param name="source">The source code to be parsed and compiled.</param>
        /// <param name="filename">The name of the source file. Used as a pouint of reference in debug/error messages only.</param>
        /// <returns></returns>
        public ShaderCompileResult CompileShaders(string source, string filename = null)
        {
            HlslIncluder includer = null;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                FileInfo fInfo = new FileInfo(filename);
                DirectoryInfo dir = fInfo.Directory;
                includer = new DefaultIncluder(_renderer.ShaderCompiler);
            }

            return _renderer.ShaderCompiler.Compile(source, filename, includer);
        }
    }
}
