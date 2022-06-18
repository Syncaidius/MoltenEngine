using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class SpriteBatcherDX11 : SpriteBatcher
    {
        class Range
        {
            public uint BufferOffset;
            public uint VertexCount;
            public ITexture2D Texture;
            public IMaterial Material;
            public SpriteFormat Format;
            public int ClipID;

            public override string ToString()
            {
                return $"Range -- Vertices: {VertexCount} -- Format: {Format}";
            }
        }

        GraphicsBuffer _buffer;
        BufferSegment _bufferData;

        BufferSegment _segment;
        Range[] _ranges;
        uint _curRange;
        uint _spriteCapacity;
        Func<DeviceContext, Range, ObjectRenderData, Material>[] _checkers;
        SpriteVertex[] _vertices;
        Material _defaultMaterial; 
        Material _defaultMaterialMS;
        Material _defaultNoTextureMaterial;
        Material _defaultLineMaterial;
        Material _defaultCircleMaterial;
        Material _defaultTriMaterial;
        Material _defaultMsdfMaterial;

        internal unsafe SpriteBatcherDX11(RendererDX11 renderer, uint capacity = 3000) : base(capacity)
        {
            // In the worst-case scenario, we can expect the number of ranges to equal the capacity. Create as many ranges.
            _ranges = new Range[capacity];
            for (uint i = 0; i < _ranges.Length; i++)
                _ranges[i] = new Range();

            _vertices = new SpriteVertex[capacity];
            _spriteCapacity = capacity;
            _buffer = new GraphicsBuffer(renderer.Device, BufferMode.DynamicDiscard, 
                BindFlag.BindShaderResource, 
                (uint)sizeof(SpriteVertex) * capacity,
                ResourceMiscFlag.ResourceMiscBufferStructured,
                StagingBufferFlags.None,
                (uint)sizeof(SpriteVertex));
            _bufferData = _buffer.Allocate<SpriteVertex>(_spriteCapacity);

            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex));

            ShaderCompileResult resultV2 = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite_v2.mfx");
            _defaultNoTextureMaterial = resultV2[ShaderClassType.Material, "sprite-no-texturev2"] as Material;
            _defaultMaterial = resultV2[ShaderClassType.Material, "sprite-texturev2"] as Material;

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite.mfx");
            _defaultMaterialMS = result[ShaderClassType.Material, "sprite-texture-ms"] as Material;
            _defaultLineMaterial = result[ShaderClassType.Material, "line"] as Material;
            _defaultCircleMaterial = result[ShaderClassType.Material, "circle"] as Material;
            _defaultTriMaterial = result[ShaderClassType.Material, "triangle"] as Material;

            ShaderCompileResult resultSdf = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite_sdf.mfx");
            _defaultMsdfMaterial = resultSdf[ShaderClassType.Material, "sprite-msdf"] as Material;

            _checkers = new Func<DeviceContext, Range, ObjectRenderData, Material>[5];
            _checkers[(int)SpriteFormat.Sprite] = CheckSpriteRange;
            _checkers[(int)SpriteFormat.MSDF] = CheckMsdfRange;
            _checkers[(int)SpriteFormat.Line] = CheckLineRange;
            _checkers[(int)SpriteFormat.Triangle] = CheckTriangleRange;
            _checkers[(int)SpriteFormat.Circle] = CheckCircleRange;
        }

        internal unsafe void Flush(DeviceContext context, RenderCamera camera, ObjectRenderData data)
        {
            if (NextID == 0)
                return;

            Range range;

            Clips[0] = (Rectangle)camera.OutputSurface.Viewport.Bounds;
            context.State.VertexBuffers[0].Value = null;

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.            
            uint i = 0;
            while (i < NextID)
            {
                // Reset vertex array pointer and ranges, so we can prepare the next batch of vertices.
                uint remaining = NextID - i;
                uint end = i + Math.Min(remaining, _spriteCapacity);
                uint start = i;

                _curRange = 0;
                range = _ranges[_curRange];

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
                        range.VertexCount = i - start;
                        _curRange++;

                        range = _ranges[_curRange];
                        start = i;
                        range.Format = item.Format;
                        range.Texture = item.Texture;
                        range.Material = item.Material;
                        range.ClipID = item.ClipID;
                    }
                }

                // Include the last range, if it has any vertices.
                range.VertexCount = i - start;
                if (range.VertexCount > 0)
                    _curRange++;

                if (_curRange > 0)
                    FlushBuffer(context, camera, data, v);
            }
            
            // Reset
            NextID = 0;
        }

        private void FlushBuffer(DeviceContext context, RenderCamera camera, ObjectRenderData data, uint vertexCount)
        {
            Range range;
            //_segment.Map(context, (buffer, stream) => stream.WriteRange(_vertices, 0, vertexCount));
            _bufferData.Map(context, (buffer, stream) => stream.WriteRange(_vertices, 0, vertexCount));

            // Draw calls
            uint bufferOffset = 0;
            for (uint i = 0; i < _curRange; i++)
            {
                range = _ranges[i];
                range.BufferOffset = bufferOffset;
                bufferOffset += range.VertexCount;

                // TODO TESTING - REMOVE LATER
                if (range.Format != SpriteFormat.Sprite && range.Format != SpriteFormat.MSDF)
                    return;

                Material mat = _checkers[(int)range.Format](context, range, data);

                mat["spriteData"].Value = _bufferData;
                mat["vertexOffset"].Value = range.BufferOffset;
                context.State.SetScissorRectangles(Clips[range.ClipID]);

                mat.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
                context.Draw(mat, range.VertexCount, VertexTopology.PointList);
            }
        }

        private Material CheckSpriteRange(DeviceContext context, Range range, ObjectRenderData data)
        {
            Material mat = range.Material as Material;

            if (range.Texture != null)
            {
                if (range.Texture.IsMultisampled)
                {
                    mat = mat ?? _defaultMaterialMS;
                    mat.Textures.DiffuseTextureMS.Value = range.Texture;
                    mat.Textures.SampleCount.Value = (uint)range.Texture.MultiSampleLevel;
                }
                else
                {
                    mat = mat ?? _defaultMaterial;
                    mat.Textures.DiffuseTexture.Value = range.Texture; 
                }

                Vector2F texSize = new Vector2F(range.Texture.Width, range.Texture.Height);
                mat.SpriteBatch.TextureSize.Value = texSize;
            }
            else
            {
                mat = mat ?? _defaultNoTextureMaterial;
            }

            return mat;
        }

        private Material CheckMsdfRange(DeviceContext context, Range range, ObjectRenderData data)
        {
            Material mat = range.Material as Material;

            if (range.Texture != null)
            {
                if (range.Texture.IsMultisampled)
                {
                    //mat = mat ?? _defaultMaterialMS;
                    //mat.Textures.DiffuseTextureMS.Value = range.Texture;
                   // mat.Textures.SampleCount.Value = (uint)range.Texture.MultiSampleLevel;
                }
                else
                {
                    mat = mat ?? _defaultMsdfMaterial;
                    mat.Textures.DiffuseTexture.Value = range.Texture;
                }

                Vector2F texSize = new Vector2F(range.Texture.Width, range.Texture.Height);
                mat.SpriteBatch.TextureSize.Value = texSize;
            }
            else
            {
                mat = mat ?? _defaultNoTextureMaterial;
            }

            return mat;
        }

        private Material CheckLineRange(DeviceContext context, Range range, ObjectRenderData data)
        {
            return _defaultLineMaterial;
        }

        private Material CheckTriangleRange(DeviceContext context, Range range, ObjectRenderData data)
        {
            return _defaultTriMaterial;
        }

        private Material CheckCircleRange(DeviceContext context, Range range, ObjectRenderData data)
        {
            return _defaultCircleMaterial;
        }

        public override void Dispose()
        {
            _defaultMaterial.Dispose();
            _defaultNoTextureMaterial.Dispose();
            _defaultMsdfMaterial.Dispose();
            _defaultLineMaterial.Dispose();
            _defaultCircleMaterial.Dispose();
            _defaultTriMaterial.Dispose();
            _segment.Dispose();
            _bufferData.Dispose();
            _buffer.Dispose();
        }
    }
}
