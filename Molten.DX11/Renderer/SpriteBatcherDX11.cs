using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    public class SpriteBatcherDX11 : SpriteBatcher
    {
        class Range
        {
            public int Start;
            public int End;
            public int VertexCount;
            public ITexture2D Texture;
            public IMaterial Material;
            public SpriteFormat Format;
        }

        BufferSegment _segment;
        Range[] _ranges;
        int _curRange;
        int _spriteCapacity;
        Action<GraphicsPipe, RenderCamera, Range>[] _flushFuncs;
        SpriteVertex2[] _vertices;

        Material _defaultMaterial;
        Material _defaultNoTextureMaterial;
        Material _defaultLineMaterial;
        Material _defaultCircleMaterial;
        Material _defaultTriMaterial;

        internal SpriteBatcherDX11(RendererDX11 renderer, int capacity = 3000) : base(capacity)
        {
            _ranges = new Range[100];
            for (int i = 0; i < _ranges.Length; i++)
                _ranges[i] = new Range();

            _vertices = new SpriteVertex2[capacity];
            _spriteCapacity = capacity;
            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex2>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex2));

            string source = null;
            string namepace = "Molten.Graphics.Assets.sprite.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = renderer.ShaderCompiler.Compile(source, namepace);
                _defaultMaterial = result["material", "sprite-texture"] as Material;
                _defaultNoTextureMaterial = result["material", "sprite-no-texture"] as Material;
                _defaultLineMaterial = result["material", "line"] as Material;
                _defaultCircleMaterial = result["material", "circle"] as Material;
                _defaultTriMaterial = result["material", "triangle"] as Material;
            }

            _flushFuncs = new Action<GraphicsPipe, RenderCamera, Range>[4]
            {
                FlushSpriteRange,
                FlushLineRange,
                FlushTriangleRange,
                FlushCircleRange,
            };
        }

        internal unsafe void Flush(GraphicsPipe pipe, RenderCamera camera, bool depthSort)
        {
            if (NextID == 0)
                return;

            Sort(camera, depthSort);
            Range range;

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.
            fixed (SpriteVertex2* vertexFixedPtr = _vertices)
            {
                SpriteVertex2* vertexPtr = vertexFixedPtr;
                SpriteItem item;

                int i = 0;
                while(i < NextID)
                {
                    int remaining = NextID - i;
                    int end = i + Math.Min(remaining, _spriteCapacity);
                    vertexPtr = vertexFixedPtr;

                    // Start the first range
                    _curRange = 0;
                    range = _ranges[_curRange];
                    range.Start = i;
                    item = Sprites[i];
                    range.Format = item.Format;
                    range.Texture = item.Texture;
                    range.Material = item.Material;
                    i++;

                    for (; i < end; i++, vertexPtr += _segment.Stride)
                    {
                        item = Sprites[i];
                        *(vertexPtr) = item.Vertex;

                        // If the current item does not match that of the current range, start a new range.
                        if (item.Texture != range.Texture ||
                            item.Material != range.Material ||
                            item.Format != range.Format)
                        {
                            range.VertexCount = i - range.Start;
                            range.End = i;
                            _curRange++;

                            if (_curRange == _ranges.Length)
                            {
                                int old = _ranges.Length;
                                Array.Resize(ref _ranges, old + (old / 2));
                                for (int o = old; o < _ranges.Length; o++)
                                    _ranges[o] = new Range();
                            }

                            range = _ranges[_curRange];
                            range.Start = i;
                            range.Format = Sprites[i].Format;
                        }
                    }

                    // Include the last range, if it has any vertices.
                    range.VertexCount = i - range.Start;
                    if (range.VertexCount > 0)
                    {
                        range.End = i;
                        _curRange++;
                    }

                    if (_curRange > 0)
                        FlushBuffer(pipe, camera);
                }
            }
        }

        private void FlushBuffer(GraphicsPipe pipe, RenderCamera camera)
        {
            Range range;
            int writeIndex = 0;

            // Map buffer segment
            _segment.Map(pipe, (buffer, stream) =>
            {
                for (int i = 0; i < _curRange; i++)
                {
                    range = _ranges[i];
                    stream.WriteRange(_vertices, writeIndex, range.VertexCount);
                    range.Start = writeIndex;
                    writeIndex += range.VertexCount;
                }
            });

            // Draw calls
            for(int i = 0; i < _curRange; i++)
            {
                range = _ranges[i];
                _flushFuncs[(int)range.Format](pipe, camera, range);
            }
        }

        private void FlushSpriteRange(GraphicsPipe pipe, RenderCamera camera, Range range)
        {
            Material mat = range.Material as Material;

            if (range.Texture != null)
            {
                mat = mat ?? _defaultMaterial;
                Vector2F texSize = new Vector2F(range.Texture.Width, range.Texture.Height);
                mat.SpriteBatch.TextureSize.Value = texSize;
                mat.Textures.DiffuseTexture.Value = range.Texture;
            }
            else
            {
                mat = mat ?? _defaultNoTextureMaterial;
            }

            mat.Object.Wvp.Value = camera.ViewProjection;
            pipe.Draw(mat, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushLineRange(GraphicsPipe pipe, RenderCamera camera, Range range)
        {
            _defaultLineMaterial.Object.Wvp.Value = camera.ViewProjection;
            pipe.Draw(_defaultLineMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushTriangleRange(GraphicsPipe pipe, RenderCamera camera, Range range)
        {
            _defaultTriMaterial.Object.Wvp.Value = camera.ViewProjection;
            pipe.Draw(_defaultTriMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushCircleRange(GraphicsPipe pipe, RenderCamera camera, Range range)
        {
            _defaultCircleMaterial.Object.Wvp.Value = camera.ViewProjection;
            pipe.Draw(_defaultCircleMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }
    }
}
