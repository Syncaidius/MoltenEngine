using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public class SpriteBatcherDX11 : SpriteBatcher
    {
        class Range
        {
            public uint Start;
            public uint End;
            public uint VertexCount;
            public ITexture2D Texture;
            public IMaterial Material;
            public SpriteFormat Format;
            public int ClipID;
        }

        BufferSegment _segment;
        Range[] _ranges;
        uint _curRange;
        uint _spriteCapacity;
        Action<DeviceContext, RenderCamera, Range, ObjectRenderData>[] _flushFuncs;
        SpriteVertex[] _vertices;
        Rectangle _vpBounds;

        Material _defaultMaterial;
        Material _defaultNoTextureMaterial;
        Material _defaultLineMaterial;
        Material _defaultCircleMaterial;
        Material _defaultTriMaterial;

        internal SpriteBatcherDX11(RendererDX11 renderer, uint capacity = 3000) : base(capacity)
        {
            // In the worst-case scenario, we can expect the number of ranges to equal the capacity. Create as many ranges.
            _ranges = new Range[capacity];
            for (uint i = 0; i < _ranges.Length; i++)
                _ranges[i] = new Range();

            _vertices = new SpriteVertex[capacity];
            _spriteCapacity = capacity;
            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex));

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite.mfx");
            _defaultMaterial = result[ShaderClassType.Material, "sprite-texture"] as Material;
            _defaultNoTextureMaterial = result[ShaderClassType.Material, "sprite-no-texture"] as Material;
            _defaultLineMaterial = result[ShaderClassType.Material, "line"] as Material;
            _defaultCircleMaterial = result[ShaderClassType.Material, "circle"] as Material;
            _defaultTriMaterial = result[ShaderClassType.Material, "triangle"] as Material;

            _flushFuncs = new Action<DeviceContext, RenderCamera, Range, ObjectRenderData>[4]
            {
                FlushSpriteRange,
                FlushLineRange,
                FlushTriangleRange,
                FlushCircleRange,
            };
        }

        internal unsafe void Flush(DeviceContext pipe, RenderCamera camera, ObjectRenderData data)
        {
            if (NextID == 0)
                return;

            Range range;

            _vpBounds = (Rectangle)camera.OutputSurface.Viewport.Bounds;
            pipe.State.VertexBuffers[0].Value = _segment;

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.            
            uint i = 0;
            while (i < NextID)
            {
                // Reset vertex array pointer and ranges, so we can prepare the next batch of vertices.
                uint remaining = NextID - i;
                uint end = i + Math.Min(remaining, _spriteCapacity);

                _curRange = 0;
                range = _ranges[_curRange];
                range.Start = i;

                ref SpriteItem item = ref Sprites[i];
                range.Format = item.Format;
                range.Texture = item.Texture;
                range.Material = item.Material;
                range.ClipID = item.ClipID;

                uint v = 0;
                for (; i < end; i++)
                {
                    item = ref Sprites[i];
                    _vertices[v++] = item.Vertex;

                    // If the current item does not match that of the current range, start a new range.
                    if (item.Texture != range.Texture ||
                        item.Material != range.Material ||
                        item.Format != range.Format ||
                        item.ClipID != range.ClipID)
                    {
                        range.VertexCount = i - range.Start;
                        range.End = i;
                        _curRange++;

                        range = _ranges[_curRange];
                        range.Start = i;
                        range.Format = item.Format;
                        range.Texture = item.Texture;
                        range.Material = item.Material;
                        range.ClipID = item.ClipID;
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
                    FlushBuffer(pipe, camera, data, v);
            }
            
            // Reset
            NextID = 0;
        }

        private void FlushBuffer(DeviceContext pipe, RenderCamera camera, ObjectRenderData data, uint vertexCount)
        {
            Range range;
            uint writeIndex = 0;

            // Map buffer segment
            _segment.Map(pipe, (buffer, stream) =>
            {
                for (uint i = 0; i < _curRange; i++)
                {
                    range = _ranges[i];
                    stream.WriteRange(_vertices, writeIndex, range.VertexCount);
                    range.Start = writeIndex;
                    writeIndex += range.VertexCount;
                }
            });

            // Draw calls
            for(uint i = 0; i < _curRange; i++)
            {
                range = _ranges[i];
                _flushFuncs[(int)range.Format](pipe, camera, range, data);
            }
        }

        private void FlushSpriteRange(DeviceContext pipe, RenderCamera camera, Range range, ObjectRenderData data)
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

            if (range.ClipID <= 0)
                pipe.State.SetScissorRectangles(_vpBounds);
            else
                pipe.State.SetScissorRectangles(Clips[range.ClipID]);

            mat.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(mat, range.VertexCount, VertexTopology.PointList, range.Start);
        }

        private void FlushLineRange(DeviceContext pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultLineMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultLineMaterial, range.VertexCount, VertexTopology.PointList, range.Start);
        }

        private void FlushTriangleRange(DeviceContext pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultTriMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultTriMaterial, range.VertexCount, VertexTopology.PointList, range.Start);
        }

        private void FlushCircleRange(DeviceContext pipe, RenderCamera camera, Range range, ObjectRenderData data)
        {
            _defaultCircleMaterial.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
            pipe.Draw(_defaultCircleMaterial, range.VertexCount, VertexTopology.PointList, range.Start);
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