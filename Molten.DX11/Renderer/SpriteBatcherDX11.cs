using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Action<GraphicsPipe, RenderCamera, Range, ObjectRenderData>[] _flushFuncs;
        SpriteVertex[] _vertices;

        Material _defaultMaterial;
        Material _defaultNoTextureMaterial;
        Material _defaultLineMaterial;
        Material _defaultCircleMaterial;
        Material _defaultTriMaterial;

        internal SpriteBatcherDX11(RendererDX11 renderer, int capacity = 3000) : base(capacity)
        {
            // In the worst-case scenario, we can expect the number of ranges to equal the capacity. Create as many ranges.
            _ranges = new Range[capacity];
            for (int i = 0; i < _ranges.Length; i++)
                _ranges[i] = new Range();

            _vertices = new SpriteVertex[capacity];
            _spriteCapacity = capacity;
            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex));

            ShaderCompileResult result = renderer.ShaderCompiler.CompileEmbedded("Molten.Graphics.Assets.sprite.mfx");
            _defaultMaterial = result["material", "sprite-texture"] as Material;
            _defaultNoTextureMaterial = result["material", "sprite-no-texture"] as Material;
            _defaultLineMaterial = result["material", "line"] as Material;
            _defaultCircleMaterial = result["material", "circle"] as Material;
            _defaultTriMaterial = result["material", "triangle"] as Material;

            _flushFuncs = new Action<GraphicsPipe, RenderCamera, Range, ObjectRenderData>[4]
            {
                FlushSpriteRange,
                FlushLineRange,
                FlushTriangleRange,
                FlushCircleRange,
            };
        }

        internal unsafe void Flush(GraphicsPipe pipe, RenderCamera camera, ObjectRenderData data)
        {
            if (NextID == 0)
                return;

            Range range;

            pipe.SetVertexSegment(_segment, 0);

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.
            // Use pointers to reduce array indexing overhead. We're potentially iterating over thousands of sprites here.
            fixed (SpriteVertex* vertexFixedPtr = _vertices)
            {
                SpriteVertex* vertexPtr = vertexFixedPtr;
                SpriteItem item;

                int i = 0;
                while(i < NextID)
                {
                    // Reset vertex array pointer and ranges, so we can prepare the next batch of vertices.
                    int remaining = NextID - i;
                    int end = i + Math.Min(remaining, _spriteCapacity);

                    vertexPtr = vertexFixedPtr;
                    _curRange = 0;
                    range = _ranges[_curRange];
                    range.Start = i;
                    item = Sprites[i];
                    range.Format = item.Format;
                    range.Texture = item.Texture;
                    range.Material = item.Material;

                    for (; i < end; i++, vertexPtr++)
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

                            range = _ranges[_curRange];
                            range.Start = i;
                            range.Format = item.Format;
                            range.Texture = item.Texture;
                            range.Material = item.Material;
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
                        FlushBuffer(pipe, camera, data);
                }
            }

            // Reset
            NextID = 0;
        }

        private void FlushBuffer(GraphicsPipe pipe, RenderCamera camera, ObjectRenderData data)
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
                _flushFuncs[(int)range.Format](pipe, camera, range, data);
            }
        }

        private void FlushSpriteRange(GraphicsPipe pipe, RenderCamera camera, Range range, ObjectRenderData data)
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

            mat.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(mat, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushLineRange(GraphicsPipe pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultLineMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultLineMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushTriangleRange(GraphicsPipe pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultTriMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultTriMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        private void FlushCircleRange(GraphicsPipe pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultCircleMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultCircleMaterial, range.VertexCount, PrimitiveTopology.PointList, range.Start);
        }

        public override void Dispose()
        {
            _defaultMaterial.Dispose();
            _defaultNoTextureMaterial.Dispose();
            _defaultLineMaterial.Dispose();
            _defaultCircleMaterial.Dispose();
            _defaultTriMaterial.Dispose();
            _segment.Dispose();
        }
    }
}
