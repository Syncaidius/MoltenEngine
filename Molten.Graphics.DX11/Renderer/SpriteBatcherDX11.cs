using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class SpriteBatcherDX11 : SpriteBatcher
    {
        GraphicsBuffer _buffer;
        BufferSegment _bufferData;

  
        Func<DeviceContext, SpriteRange, ObjectRenderData, Material>[] _checkers;
        Material _matDefault; 
        Material _matDefaultMS;
        Material _matDefaultNoTexture;
        Material _matLine;
        Material _matGrid;
        Material _matCircle;
        Material _matCircleNoTexture;
        Material _matTriangle;
        Material _matMsdf;

        internal unsafe SpriteBatcherDX11(RendererDX11 renderer, uint capacity = 3000) : base(capacity)
        {
            _buffer = new GraphicsBuffer(renderer.Device, BufferMode.DynamicDiscard, 
                BindFlag.BindShaderResource, 
                (uint)sizeof(GpuData) * capacity,
                ResourceMiscFlag.ResourceMiscBufferStructured,
                StagingBufferFlags.None,
                (uint)sizeof(GpuData));
            _bufferData = _buffer.Allocate<GpuData>(capacity);

            ShaderCompileResult resultV2 = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite_v2.mfx");
            _matDefaultNoTexture = resultV2[ShaderClassType.Material, "sprite-no-texture"] as Material;
            _matDefault = resultV2[ShaderClassType.Material, "sprite-texture"] as Material;
            _matCircle = resultV2[ShaderClassType.Material, "circle"] as Material;
            _matCircleNoTexture = resultV2[ShaderClassType.Material, "circle-no-texture"] as Material;
            _matLine = resultV2[ShaderClassType.Material, "line"] as Material;
            _matGrid = resultV2[ShaderClassType.Material, "grid"] as Material;

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite.mfx");
            _matDefaultMS = result[ShaderClassType.Material, "sprite-texture-ms"] as Material;
            _matTriangle = result[ShaderClassType.Material, "triangle"] as Material;

            ShaderCompileResult resultSdf = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite_sdf.mfx");
            _matMsdf = resultSdf[ShaderClassType.Material, "sprite-msdf"] as Material;

            _checkers = new Func<DeviceContext, SpriteRange, ObjectRenderData, Material>[6];
            _checkers[(int)ItemType.Sprite] = CheckSpriteRange;
            _checkers[(int)ItemType.MSDF] = CheckMsdfRange;
            _checkers[(int)ItemType.Line] = CheckLineRange;
            _checkers[(int)ItemType.Triangle] = CheckTriangleRange;
            _checkers[(int)ItemType.Ellipse] = CheckEllipseRange;
            _checkers[(int)ItemType.Grid] = CheckGridRange;
        }

        internal unsafe void Flush(DeviceContext context, RenderCamera camera, ObjectRenderData data)
        {
            if (NextID == 0)
                return;

            ClipStack[0] = (Rectangle)camera.OutputSurface.Viewport.Bounds;
            context.State.VertexBuffers[0].Value = null;

            ProcessBatches((rangeCount, vertexStartIndex, numVerticesInBuffer) => 
                FlushBuffer(context, camera, data, rangeCount, vertexStartIndex, numVerticesInBuffer));
        }

        private void FlushBuffer(DeviceContext context, RenderCamera camera, ObjectRenderData data, uint rangeCount, uint vertexStartIndex, uint vertexCount)
        {
            SpriteRange range;
            _bufferData.Map(context, (buffer, stream) => stream.WriteRange(Data, vertexStartIndex, vertexCount));

            // Draw calls
            uint bufferOffset = 0;
            for (uint i = 0; i < rangeCount; i++)
            {
                range = Ranges[i];
                range.BufferOffset = bufferOffset;
                bufferOffset += range.VertexCount;

                // TODO TESTING - REMOVE LATER
                if (range.Type != ItemType.Sprite && 
                    range.Type != ItemType.MSDF && 
                    range.Type != ItemType.Ellipse &&
                    range.Type != ItemType.Line &&
                    range.Type != ItemType.Grid)
                    continue;

                Material mat = (range.Material as Material) ?? _checkers[(int)range.Type](context, range, data);

                mat["spriteData"].Value = _bufferData;
                mat["vertexOffset"].Value = range.BufferOffset;

                // Set common material properties
                if (range.Texture != null)
                {
                    if (range.Texture.IsMultisampled)
                    {
                        mat.Textures.DiffuseTextureMS.Value = range.Texture;
                        mat.Textures.SampleCount.Value = (uint)range.Texture.MultiSampleLevel;
                    }
                    else
                    {
                        mat.Textures.DiffuseTexture.Value = range.Texture;
                    }

                    Vector2F texSize = new Vector2F(range.Texture.Width, range.Texture.Height);
                    mat.SpriteBatch.TextureSize.Value = texSize;
                }

                context.State.SetScissorRectangles(ClipStack[range.ClipID]);

                mat.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
                context.Draw(mat, range.VertexCount, VertexTopology.PointList);
            }
        }

        private Material CheckSpriteRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            if (range.Texture != null)
                return range.Texture.IsMultisampled ? _matDefaultMS : _matDefault;
            else
                return _matDefaultNoTexture;
        }

        private Material CheckMsdfRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            if (range.Texture != null)
            {
                if (range.Texture.IsMultisampled)
                    return _matDefaultMS; // TODO Implement MSDF Multi-sampling
                else
                    return _matMsdf;
            }
            else
            {
                return _matDefaultNoTexture;
            }
        }

        private Material CheckLineRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return _matLine;
        }

        private Material CheckTriangleRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return _matTriangle;
        }

        private Material CheckEllipseRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        private Material CheckGridRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return _matGrid; // range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        public override void Dispose()
        {
            _matDefault.Dispose();
            _matDefaultNoTexture.Dispose();
            _matMsdf.Dispose();
            _matGrid.Dispose();
            _matLine.Dispose();
            _matCircle.Dispose();
            _matCircleNoTexture.Dispose();
            _matTriangle.Dispose();

            _bufferData.Dispose();
            _buffer.Dispose();
        }
    }
}
