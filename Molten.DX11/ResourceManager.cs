using SharpDX.DXGI;
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
        GraphicsDevice _device;
        RendererDX11 _renderer;
        List<SpriteFont> _fontTable;

        internal ResourceManager(RendererDX11 renderer)
        {
            _device = renderer.Device;
            _renderer = renderer;
            _fontTable = new List<SpriteFont>();
        }

        public IDepthSurface CreateDepthSurface(
            int width, 
            int height, 
            int mipCount = 1, 
            int arraySize = 1, 
            int sampleCount = 1, 
            DepthFormat format = DepthFormat.R24G8_Typeless, 
            TextureFlags flags = TextureFlags.None)
        {
            return new DepthSurface(_device, width, height, mipCount, arraySize, sampleCount, format, flags);
        }

        public IWindowSurface CreateFormSurface(string formTitle, int mipCount = 1, int sampleCount = 1)
        {
            return new RenderFormSurface(formTitle, _device, mipCount);
        }

        public IRenderSurface CreateSurface(
            int width, 
            int height, 
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, 
            int mipCount = 1,
            int arraySize = 1, 
            int sampleCount = 1,
            TextureFlags flags = TextureFlags.None)
        {
            return new RenderSurface(_device, width, height, (Format)format, mipCount, arraySize, sampleCount, flags);
        }

        public ITexture CreateTexture1D(Texture1DProperties properties)
        {
            return new TextureAsset1D(_device, properties.Width, properties.Format.ToApi(), properties.MipMapLevels, properties.ArraySize, properties.Flags);
        }

        public ITexture CreateTexture1D(TextureData data)
        {
            TextureAsset1D tex = new TextureAsset1D(_device, data.Width, data.Format.ToApi(), data.MipMapCount, data.ArraySize, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapCount, data.ArraySize);
            return tex;
        }

        public ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            return new TextureAsset2D(_device, 
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
            TextureAsset2D tex = new TextureAsset2D(_device, 
                data.Width, 
                data.Height, 
                data.Format.ToApi(), 
                data.MipMapCount, 
                data.ArraySize, 
                data.Flags, 
                data.SampleCount);

            tex.SetData(data, 0, 0, data.MipMapCount, data.ArraySize);
            return tex;
        }

        public ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            int cubeCount = Math.Max(properties.ArraySize / 6, 1);
            return new TextureAssetCube(_device, properties.Width, properties.Height, properties.Format.ToApi(), properties.MipMapLevels, cubeCount, properties.Flags);
        }

        public ITextureCube CreateTextureCube(TextureData data)
        {
            int cubeCount = Math.Max(data.ArraySize / 6, 1);
            TextureAssetCube tex = new TextureAssetCube(_device, data.Width, data.Height, data.Format.ToApi(), data.MipMapCount, cubeCount, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapCount, data.ArraySize);
            return tex;
        }

        public TextureReader GetDefaultTextureReader(FileInfo file)
        {
            return null; // new DefaultTextureReader(_device);
        }

        public void SaveAsBitmap(Stream stream, TextureData data)
        {
            throw new NotImplementedException();
        }

        public void SaveAsDDS(DDSFormat format, Stream stream, TextureData data)
        {
            throw new NotImplementedException();
        }

        public void SaveAsJpeg(Stream stream, TextureData data)
        {
            throw new NotImplementedException();
        }

        public void SaveAsPng(Stream stream, TextureData data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            for (int i = 0; i < _fontTable.Count; i++)
                _fontTable[i].Dispose();

            _fontTable.Clear();
        }

        public IMesh<T> CreateMesh<T>(int maxVertices, VertexTopology topology = VertexTopology.TriangleList, bool dynamic = false) 
            where T : struct, IVertexType
        {
            return new Mesh<T>(_renderer, maxVertices, topology, dynamic);
        }

        public IIndexedMesh<T> CreateIndexedMesh<T>(
            int maxVertices, 
            int maxIndices, 
            VertexTopology topology = VertexTopology.TriangleList, 
            IndexBufferFormat indexFormat = IndexBufferFormat.Unsigned32Bit, 
            bool dynamic = false)
            where T : struct, IVertexType
        {
            return new IndexedMesh<T>(_renderer, maxVertices, maxIndices, topology, indexFormat, dynamic);
        }

        /// <summary>Compiels a set of shaders from the provided source string.</summary>
        /// <param name="source">The source code to be parsed and compiled.</param>
        /// <param name="filename">The name of the source file. Used as a point of reference in debug/error messages only.</param>
        /// <returns></returns>
        public ShaderCompileResult CreateShaders(string source, string filename = null)
        {
            ShaderCompileResult result = _renderer.ShaderCompiler.Compile(source, filename);

            foreach (string error in result.Errors)
                _renderer.Device.Log.WriteError(error);

            foreach (string warning in result.Warnings)
                _renderer.Device.Log.WriteWarning(warning);

            return result;
        }
    }
}
