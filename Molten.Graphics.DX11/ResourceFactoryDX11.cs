using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>DirectX 11 implementation of <see cref="ResourceFactory"/>.</summary>
    public class ResourceFactoryDX11 : ResourceFactory
    {
        RendererDX11 _renderer;
        Device _device;
        List<SpriteFont> _fontTable;

        internal ResourceFactoryDX11(RendererDX11 renderer) : 
            base(renderer, renderer.ShaderCompiler)
        {
            _renderer = renderer;
            _device = _renderer.Device;
            _fontTable = new List<SpriteFont>();
        }

        public override IDepthStencilSurface CreateDepthSurface(
            uint width,
            uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            TextureFlags flags = TextureFlags.None)
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new DepthStencilSurface(_renderer, width, height, format, mipCount, arraySize, aaLevel, msaa, flags);
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            return new RenderFormSurface(formTitle, formName, _renderer, mipCount);
        }

        public override INativeSurface CreateControlSurface(string formTitle, string controlName, uint mipCount = 1)
        {
            return new RenderControlSurface(formTitle, controlName, _renderer, mipCount);
        }

        public override IRenderSurface2D CreateSurface(
            uint width,
            uint height,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            TextureFlags flags = TextureFlags.None)
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new RenderSurface2D(_renderer, width, height, (Format)format, mipCount, arraySize, aaLevel, msaa, flags);
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties)
        {
            return new Texture1D(_renderer, properties.Width, properties.Format.ToApi(), properties.MipMapLevels, properties.ArraySize, properties.Flags);
        }

        public override ITexture CreateTexture1D(TextureData data)
        {
            Texture1D tex = new Texture1D(_renderer, data.Width, data.Format.ToApi(), data.MipMapLevels, data.ArraySize, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            return new Texture2D(_renderer,
                properties.Width,
                properties.Height,
                properties.Format.ToApi(),
                properties.MipMapLevels,
                properties.ArraySize,
                properties.Flags,
                properties.MultiSampleLevel);
        }

        public override ITexture2D CreateTexture2D(TextureData data)
        {
            Texture2D tex = new Texture2D(_renderer,
                data.Width,
                data.Height,
                data.Format.ToApi(),
                data.MipMapLevels,
                data.ArraySize,
                data.Flags,
                data.MultiSampleLevel);

            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties)
        {
            return new Texture3D(_renderer,
                properties.Width,
                properties.Height,
                properties.Depth,
                properties.Format.ToApi(),
                properties.MipMapLevels,
                properties.Flags);
        }

        public override ITexture3D CreateTexture3D(TextureData data)
        {
            throw new NotImplementedException();

            // TODO TextureData needs support for 3D data

            /*Texture3D tex = new Texture3D(_renderer,
                data.Width,
                data.Height,
                data.Depth,
                data.Format.ToApi(),
                data.MipMapLevels,
                data.Flags);

            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;*/
        }

        public override ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            uint cubeCount = Math.Max(properties.ArraySize / 6, 1);
            return new TextureCubeDX11(_renderer, properties.Width, properties.Height, properties.Format.ToApi(), properties.MipMapLevels, cubeCount, properties.Flags);
        }

        public override ITextureCube CreateTextureCube(TextureData data)
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
        public override void ResolveTexture(ITexture source, ITexture destination)
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
        public override void ResolveTexture(ITexture source, ITexture destination,
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

        protected override void OnDispose()
        {
            for (int i = 0; i < _fontTable.Count; i++)
                _fontTable[i].Dispose();

            _fontTable.Clear();
        }

        public override ISpriteRenderer CreateSpriteRenderer(Action<SpriteBatcher> callback = null)
        {
            return new SpriteRendererDX11(_renderer.Device, callback);
        }

        public override IMesh<GBufferVertex> CreateMesh(uint maxVertices, VertexTopology topology, bool dynamic)
        {
            return new StandardMesh(_renderer, maxVertices, topology, dynamic);
        }

        public override IIndexedMesh<GBufferVertex> CreateIndexedMesh(uint maxVertices,
            uint maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            bool dynamic = false)
        {
            return new StandardIndexedMesh(_renderer, maxVertices, maxIndices, topology, IndexBufferFormat.Unsigned32Bit, dynamic);
        }

        public override IMesh<T> CreateMesh<T>(uint maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false) 
        {
            return new Mesh<T>(_renderer, maxVertices, topology, dynamic);
        }

        public  override IIndexedMesh<T> CreateIndexedMesh<T>(
            uint maxVertices, 
            uint maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit, 
            bool dynamic = false)
        {
            return new IndexedMesh<T>(_renderer, maxVertices, maxIndices, topology, indexFormat, dynamic);
        }
    }
}
