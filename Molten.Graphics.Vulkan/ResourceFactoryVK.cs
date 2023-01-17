using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ResourceFactoryVK : ResourceFactory
    {
        RendererVK _renderer;

        internal ResourceFactoryVK(RendererVK renderer, ShaderCompiler sCompiler) : 
            base(renderer, sCompiler)
        {
            _renderer = renderer;
        }

        public override INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        public override IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8_Typeless, uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, TextureFlags flags = TextureFlags.None)
        {
            throw new NotImplementedException();
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            return new WindowSurfaceVK(_renderer.NativeDevice, GraphicsFormat.B8G8R8A8_UNorm, formTitle, 1024, 800);
        }

        public override IIndexedMesh<GBufferVertex> CreateIndexedMesh(uint maxVertices, uint maxIndices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public override IIndexedMesh<T> CreateIndexedMesh<T>(uint maxVertices, uint maxIndices, VertexTopology topology = VertexTopology.TriangleList, IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public override IMesh<GBufferVertex> CreateMesh(uint maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public override IMesh<T> CreateMesh<T>(uint maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false)
        {
            throw new NotImplementedException();
        }

        public override ISpriteRenderer CreateSpriteRenderer(Action<SpriteBatcher> callback = null)
        {
            throw new NotImplementedException();
        }

        public override IRenderSurface2D CreateSurface(uint width, uint height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, TextureFlags flags = TextureFlags.None)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(TextureData data)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(TextureData data)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(TextureData data)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(TextureData data)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(ITexture source, ITexture destination)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(ITexture source, ITexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}
