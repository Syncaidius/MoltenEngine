using System;

namespace Molten.Graphics
{
    public class ResourceManagerGL : IResourceManager
    {
        RendererGL _renderer;

        internal ResourceManagerGL(RendererGL renderer)
        {
            _renderer = renderer;
        }

        public ShaderCompileResult CompileShaders(string source, string filename = null)
        {
            throw new NotImplementedException();
        }

        public INativeSurface CreateControlSurface(string controlTitle, string controlName, int mipCount = 1, int sampleCount = 1)
        {
            throw new NotImplementedException();
        }

        public IDepthStencilSurface CreateDepthSurface(int width, int height, DepthFormat format = DepthFormat.R24G8_Typeless, int mipCount = 1, int arraySize = 1, int sampleCount = 1, TextureFlags flags = TextureFlags.None)
        {
            throw new NotImplementedException();
        }

        public INativeSurface CreateFormSurface(string formTitle, string formName, int mipCount = 1, int sampleCount = 1)
        {
            throw new NotImplementedException();
        }

        public IIndexedMesh<GBufferVertex> CreateIndexedMesh(int maxVertices, int maxIndices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public IIndexedMesh<T> CreateIndexedMesh<T>(int maxVertices, int maxIndices, VertexTopology topology = VertexTopology.TriangleList, IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit, bool dynamic = false) where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }

        public IMesh<GBufferVertex> CreateMesh(int maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public IMesh<T> CreateMesh<T>(int maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false) where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }

        public ISpriteRenderer CreateSpriteRenderer(Action<SpriteBatcher> callback = null)
        {
            throw new NotImplementedException();
        }

        public IRenderSurface CreateSurface(int width, int height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, int mipCount = 1, int arraySize = 1, int sampleCount = 1, TextureFlags flags = TextureFlags.None)
        {
            throw new NotImplementedException();
        }

        public ITexture CreateTexture1D(Texture1DProperties properties)
        {
            return new Texture1DGL(_renderer, properties.Width, properties.Format, properties.MipMapLevels, properties.ArraySize, properties.Flags);
        }

        public ITexture CreateTexture1D(TextureData data)
        {
            Texture1DGL tex = new Texture1DGL(_renderer, data.Width, data.Format, data.MipMapLevels, data.ArraySize, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            return new Texture2DGL(_renderer,
                properties.Width,
                properties.Height,
                properties.Format,
                properties.MipMapLevels,
                properties.ArraySize,
                properties.Flags, properties.SampleCount);
        }

        public ITexture2D CreateTexture2D(TextureData data)
        {
            Texture2DGL tex = new Texture2DGL(_renderer, data.Width, data.Height, data.Format, data.MipMapLevels, data.ArraySize, data.Flags, data.SampleCount);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            throw new NotImplementedException();
        }

        public ITextureCube CreateTextureCube(TextureData data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ResolveTexture(ITexture source, ITexture destination)
        {
            throw new NotImplementedException();
        }

        public void ResolveTexture(ITexture source, ITexture destination, int sourceMipLevel, int sourceArraySlice, int destMiplevel, int destArraySlice)
        {
            throw new NotImplementedException();
        }
    }
}
