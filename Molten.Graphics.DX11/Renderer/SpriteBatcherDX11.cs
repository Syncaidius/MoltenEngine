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
        Material _matMsdf;

        internal unsafe SpriteBatcherDX11(RendererDX11 renderer, uint dataCapacity, uint rangeCapcity) : base(dataCapacity, rangeCapcity)
        {
            _buffer = new GraphicsBuffer(renderer.Device, BufferMode.DynamicDiscard, 
                BindFlag.ShaderResource, 
                (uint)sizeof(GpuData) * dataCapacity,
                ResourceMiscFlag.BufferStructured,
                StagingBufferFlags.None,
                (uint)sizeof(GpuData));
            _bufferData = _buffer.Allocate<GpuData>(dataCapacity);

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite.mfx");
            _matDefaultNoTexture = result[ShaderClassType.Material, "sprite-no-texture"] as Material;
            _matDefault = result[ShaderClassType.Material, "sprite-texture"] as Material;
            _matCircle = result[ShaderClassType.Material, "circle"] as Material;
            _matCircleNoTexture = result[ShaderClassType.Material, "circle-no-texture"] as Material;
            _matLine = result[ShaderClassType.Material, "line"] as Material;
            _matGrid = result[ShaderClassType.Material, "grid"] as Material;
            //_matDefaultMS = result[ShaderClassType.Material, "sprite-texture-ms"] as Material;

            ShaderCompileResult resultSdf = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "sprite_sdf.mfx");
            _matMsdf = resultSdf[ShaderClassType.Material, "sprite-msdf"] as Material;

            _checkers = new Func<DeviceContext, SpriteRange, ObjectRenderData, Material>[7];
            _checkers[(int)RangeType.None] = NoCheckRange;
            _checkers[(int)RangeType.Sprite] = CheckSpriteRange;
            _checkers[(int)RangeType.MSDF] = CheckMsdfRange;
            _checkers[(int)RangeType.Line] = CheckLineRange;
            _checkers[(int)RangeType.Ellipse] = CheckEllipseRange;
            _checkers[(int)RangeType.Grid] = CheckGridRange;
        }

        internal unsafe void Flush(DeviceContext context, RenderCamera camera, ObjectRenderData data)
        {
            if (DataCount == 0)
                return;

            context.State.VertexBuffers[0].Value = null;

            ProcessBatches(camera, (firstRangeID, rangeCount, firstDataID, flushCount) => 
                FlushBuffer(context, camera, data, firstRangeID, rangeCount, firstDataID, flushCount));
        }

        private void FlushBuffer(DeviceContext context, RenderCamera camera, ObjectRenderData data, uint firstRangeID, uint rangeCount, uint vertexStartIndex, uint vertexCount)
        {
            SpriteRange range;
            _bufferData.Map(context, (buffer, stream) => stream.WriteRange(Data, vertexStartIndex, vertexCount));

            // Draw calls
            uint bufferOffset = 0;
            uint rangeID = firstRangeID;

            for (uint i = 0; i < rangeCount; i++)
            {
                range = Ranges[rangeID++];
                if (range.Type == RangeType.None)
                    continue;

                Material mat = (range.Material as Material) ?? _checkers[(int)range.Type](context, range, data);

                mat["spriteData"].Value = _bufferData;
                mat["vertexOffset"].Value = bufferOffset;

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

                context.State.SetScissorRectangles(range.Clip);

                mat.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
                context.Draw(mat, range.VertexCount, VertexTopology.PointList);

                bufferOffset += range.VertexCount;
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

        private Material CheckEllipseRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        private Material CheckGridRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return _matGrid; // range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        private Material NoCheckRange(DeviceContext context, SpriteRange range, ObjectRenderData data)
        {
            return null;
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

            _bufferData.Dispose();
            _buffer.Dispose();
        }
    }
}
