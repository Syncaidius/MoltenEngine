using System;
using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public partial class SpriteBatcher : IDisposable
    {
        protected delegate void FlushRangeCallback(uint firstRangeID, uint rangeCount, uint dataStartIndex, uint numDataInBuffer);

        protected struct SpriteRange
        {
            public uint VertexCount;        // 4-bytes
            public ITexture2D Texture;      // 8-bytes (64-bit reference)
            public HlslShader Shader;       // 8-bytes (64-bit reference)
            public Rectangle Clip;          // Clipping rectangle.
            public RangeType Type;          // 1-byte

            public override string ToString()
            {
                return $"Range -- Vertices: {VertexCount} -- Format: {Type}";
            }
        }

        protected enum RangeType : int
        {
            /// <summary>
            /// Range contains no sprites and will not draw.
            /// </summary>
            None = 0,

            /// <summary>
            /// A textured sprite or untextured rectangle.
            /// </summary>
            Sprite = 1, // Textured or untextured (rectangle) sprites

            /// <summary>
            /// Multi-channel signed-distance field.
            /// </summary>
            MSDF = 2,

            /// <summary>
            /// Untextured lines.
            /// </summary>
            Line = 3,

            /// <summary>
            /// Ellipse or circular shape. It can be textured or untextured.
            /// </summary>
            Ellipse = 4,

            /// <summary>
            /// Textured or untextured grid.
            /// </summary>
            Grid = 5,
        }

        static Vector2F DEFAULT_ORIGIN_CENTER = new Vector2F(0.5f);

        protected Rectangle[] ClipStack;
        protected GpuData[] Data;
        protected SpriteRange[] Ranges;

        ushort _curClipID;
        uint _curRange;
        uint _dataCount;

        IStructuredBuffer _buffer;

        Func<GraphicsCommandQueue, SpriteRange, ObjectRenderData, HlslShader>[] _checkers;
        HlslShader _matDefault;
        HlslShader _matDefaultMS;
        HlslShader _matDefaultNoTexture;
        HlslShader _matLine;
        HlslShader _matGrid;
        HlslShader _matCircle;
        HlslShader _matCircleNoTexture;
        HlslShader _matMsdf;

        /// <summary>
        /// Placeholder for internal rectangle/sprite styling.
        /// </summary>
        RectStyle _rectStyle;

        public unsafe SpriteBatcher(RenderService renderer, uint dataCapacity, uint rangeCapacity)
        {
            _rectStyle = RectStyle.Default;

            FlushCapacity = dataCapacity;
            Data = new GpuData[dataCapacity];

            rangeCapacity = Math.Min(rangeCapacity, 20);
            Ranges = new SpriteRange[rangeCapacity];

            ClipStack = new Rectangle[256];
            Reset();

            _buffer = renderer.Device.CreateStructuredBuffer<GpuData>(GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.Discard, dataCapacity, false, true);

            ShaderCompileResult result = renderer.Device.LoadEmbeddedShader("Molten.Assets", "sprite.mfx");
            _matDefaultNoTexture = result["sprite-no-texture"];
            _matDefault = result["sprite-texture"];
            _matCircle = result["circle"];
            _matCircleNoTexture = result["circle-no-texture"];
            _matLine = result["line"];
            _matGrid = result["grid"];
            //_matDefaultMS = result[ShaderClassType.Material, "sprite-texture-ms"] as Material;

            ShaderCompileResult resultSdf = renderer.Device.LoadEmbeddedShader("Molten.Assets", "sprite_sdf.mfx");
            _matMsdf = resultSdf["sprite-msdf"];

            _checkers = new Func<GraphicsCommandQueue, SpriteRange, ObjectRenderData, HlslShader>[7];
            _checkers[(int)RangeType.None] = NoCheckRange;
            _checkers[(int)RangeType.Sprite] = CheckSpriteRange;
            _checkers[(int)RangeType.MSDF] = CheckMsdfRange;
            _checkers[(int)RangeType.Line] = CheckLineRange;
            _checkers[(int)RangeType.Ellipse] = CheckEllipseRange;
            _checkers[(int)RangeType.Grid] = CheckGridRange;
        }

        /// <summary>
        /// Pushes a new clipping <see cref="Rectangle"/> into the current <see cref="SpriteBatcher"/>.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>Returns true if the clip was valid. 
        /// False will be returned if <paramref name="bounds"/> is invalid, or if the clip is outside of a previously-pushed clip.</returns>
        public bool PushClip(Rectangle bounds)
        {
            if (_curClipID == ClipStack.Length)
                Array.Resize(ref ClipStack, ClipStack.Length * 2);

            // Cull the bounds to the current clip, if any.
            if (_curClipID > 0)
            {
                Rectangle cur = ClipStack[_curClipID];

                bounds.X = int.Clamp(bounds.X, cur.X, cur.Right);
                bounds.Y = int.Clamp(bounds.Y, cur.Y, cur.Bottom);
                bounds.Right = int.Clamp(bounds.Right, cur.X, cur.Right);
                bounds.Bottom = int.Clamp(bounds.Bottom, cur.Y, cur.Bottom);
            }

            if (bounds.Width > 0 && bounds.Height > 0)
            {
                ClipStack[++_curClipID] = bounds;

                // Insert a range with the newest clipping rectangle.
                ref SpriteRange range = ref GetRange(RangeType.None);
                range.Clip = bounds;

                return true;
            }

            return false;
        }

        public void PopClip()
        {
            if (_curClipID == 0)
                throw new Exception("There are no clips available to pop");

            _curClipID--;

            ref SpriteRange range = ref GetRange(RangeType.None);
            range.Clip = ClipStack[_curClipID];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ref GpuData GetData(RangeType type, ITexture2D texture, HlslShader shader)
        {
            ref SpriteRange range = ref Ranges[_curRange];
            if (range.Type != type ||
                range.Texture != texture ||
                range.Shader != shader ||
                range.VertexCount == FlushCapacity)
            {
                ref Rectangle curClip = ref range.Clip;
                range = ref GetRange(type);
                range.Texture = texture;
                range.Shader = shader;
                range.Clip = curClip;
            }

            range.VertexCount++;

            if (_dataCount == Data.Length) // Increase length by 50%
                Array.Resize(ref Data, Data.Length + (Data.Length / 2));

            return ref Data[_dataCount++];
        }

        /// <summary>Inserts a new sprite range, without any <see cref="GpuData"/>. 
        /// This is useful for state changes, which do not require any sprites to be drawn. e.g. pushing or popping clip rectangles.</summary>
        /// <param name="type"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref SpriteRange GetRange(RangeType type)
        {
            _curRange++;

            if(_curRange == Ranges.Length) // Increase length by 50%
                Array.Resize(ref Ranges, Ranges.Length + (Ranges.Length / 2));

            ref SpriteRange range = ref Ranges[_curRange];
            range.Type = type;
            range.VertexCount = 0;

            return ref range;
        }

        internal void Reset(Rectangle bounds)
        {
            _curClipID = 0;
            ClipStack[_curClipID] = bounds;
            Reset();
        }

        private void Reset()
        {
            _curRange = 0;
            _curClipID = 0;
            _dataCount = 0;
            Ranges[_curRange].Type = RangeType.None;
            Ranges[_curRange].Clip = ClipStack[_curClipID];
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, Color lineColor, float lineThickness, 
            ITexture2D cellTexture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = new Vector2F(lineThickness),
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, shader, arraySlice, surfaceSlice);
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, 
            Color lineColor, Vector2F lineThickness, ITexture2D cellTexture = null, HlslShader shader = null, 
            uint arraySlice = 0, uint surfaceSlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = lineThickness,
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, shader, arraySlice, surfaceSlice);
        }

        public unsafe void DrawGrid(RectangleF bounds, 
            Vector2F cellSize, 
            float rotation,
            Vector2F origin, 
            ref GridStyle style, 
            ITexture2D cellTexture = null, 
            HlslShader shader = null, 
            uint arraySlice = 0, 
            uint surfaceSlice = 0)
        {
            RectangleF source = cellTexture != null ? new RectangleF(0, 0, cellTexture.Width, cellTexture.Height) : RectangleF.Empty;
            float cellIncX = bounds.Size.X / cellSize.X;
            float cellIncY = bounds.Size.Y / cellSize.Y;

            ref GpuData data = ref GetData(RangeType.Grid, cellTexture, shader);
            data.Position = bounds.TopLeft;
            data.Rotation = rotation;
            data.Size = bounds.Size;
            data.Color1 = style.CellColor;
            data.Color2 = style.LineColor;
            data.Origin = origin;
            data.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.
            data.Array.SrcArraySlice = arraySlice;
            data.Array.DestSurfaceSlice = surfaceSlice;

            data.Extra.D1 = style.LineThickness.X / data.Size.X; // Convert to UV coordinate system (0 - 1) range
            data.Extra.D2 = style.LineThickness.Y / data.Size.Y; // Convert to UV coordinate system (0 - 1) range
            data.Extra.D3 = cellIncX / bounds.Size.X;
            data.Extra.D4 = cellIncY / bounds.Size.Y;
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color">Sets the color of the sprite. This overrides <see cref="SpriteStyle.PrimaryColor"/> of the active <see cref="SpriteStyle"/>.</param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF destination, Color color, ITexture2D texture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            _rectStyle.FillColor = color;
            _rectStyle.BorderThickness.Zero();

            Draw(texture,
                texture != null ? new RectangleF(0,0,texture.Width, texture.Height) : RectangleF.Empty,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                ref _rectStyle,
                shader,
                arraySlice,
                surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color">Sets the color of the sprite. This overrides <see cref="SpriteStyle.PrimaryColor"/> of the active <see cref="SpriteStyle"/>.</param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF source, RectangleF destination, Color color, ITexture2D texture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            _rectStyle.FillColor = color;
            _rectStyle.BorderThickness.Zero();

            Draw(texture,
                source,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                ref _rectStyle,
                shader,
                arraySlice,
                surfaceSlice);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        /// <param name="surfaceSlice"></param>
        public void Draw(RectangleF source, RectangleF destination, ref RectStyle style, ITexture2D texture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            Draw(texture,
                source,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                ref style,
                shader,
                arraySlice,
                surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="destination"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF destination, ref RectStyle style, ITexture2D texture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, 0, Vector2F.Zero, ref style, shader, arraySlice, surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="destination"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param><
        public void Draw(RectangleF destination, float rotation, Vector2F origin, 
            ref RectStyle style, 
            ITexture2D texture = null, 
            HlslShader shader = null, 
            uint arraySlice = 0, 
            uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, rotation, origin, ref style, shader, arraySlice, surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        /// <param name="surfaceSlice"></param>
        public void Draw(Vector2F position, ref RectStyle style, ITexture2D texture = null, HlslShader shader = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), 0, Vector2F.Zero, ref style, shader, arraySlice, surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="sprite">The <see cref="Sprite"/> to be added.</param>
        public void Draw(Sprite sprite)
        {
            Draw(sprite.Data.Texture,
                sprite.Data.Source,
                sprite.Position,
                sprite.Data.Source.Size * sprite.Scale,
                sprite.Rotation,
                sprite.Origin,
                ref sprite.Data.Style,
                sprite.Shader,
                sprite.Data.ArraySlice,
                sprite.TargetSurfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material">The material to use when rendering the sprite.</param>
        /// <param name="arraySlice">The texture array slice containing the source texture.</param>
        /// <param name="surfaceSlice">The destination slice of a bound <see cref="IRenderSurface"/>. This is only used when rendering to a render surface array.</param>
        public void Draw(Vector2F position, float rotation, Vector2F origin, ITexture2D texture, ref RectStyle style, HlslShader shader = null, float arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), rotation, origin, ref style, shader, arraySlice, surfaceSlice);
        }

        /// <summary>
        /// Adds a sprite to the batch using 2D coordinates.
        /// </summary>>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="size">The width and height of the sprite..</param>
        /// <param name="style"></param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material">The material to use when rendering the sprite.</param>
        /// <param name="arraySlice">The texture array slice containing the source texture.</param>
        public unsafe void Draw(ITexture2D texture,
            RectangleF source,
            Vector2F position,
            Vector2F size,
            float rotation,
            Vector2F origin,
            ref RectStyle style,
            HlslShader shader,
            float arraySlice, uint surfaceSlice)
        {
            ref GpuData vertex = ref GetData(RangeType.Sprite, texture, shader);
            vertex.Position = position;
            vertex.Rotation = rotation;
            vertex.Array.SrcArraySlice = arraySlice;
            vertex.Array.DestSurfaceSlice = surfaceSlice;
            vertex.Size = size;
            vertex.Color1 = style.FillColor;
            vertex.Color2 = style.BorderColor;
            vertex.Origin = origin;
            vertex.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.

            if (vertex.Color2.A > 0)
            {
                vertex.Extra.D1 = style.BorderThickness.Left / size.X; // Convert to UV coordinate system (0 - 1) range
                vertex.Extra.D2 = style.BorderThickness.Top / size.Y; // Convert to UV coordinate system (0 - 1) range
                vertex.Extra.D3 = style.BorderThickness.Right / size.X; // Convert to UV coordinate system (0 - 1) range
                vertex.Extra.D4 = style.BorderThickness.Bottom / size.Y; // Convert to UV coordinate system (0 - 1) range
            }
            else
            {
                vertex.Extra.D1 = 0; 
                vertex.Extra.D2 = 0; 
                vertex.Extra.D3 = 0;
                vertex.Extra.D4 = 0;
            }
        }

        public void Flush(GraphicsCommandQueue cmd, RenderCamera camera, ObjectRenderData data)
        {
            if (_dataCount > 0)
            {
                cmd.VertexBuffers[0].Value = null;

                SpriteRange t = new SpriteRange();
                ref SpriteRange range = ref t;

                // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.            
                uint dataID = 0;
                uint rangeID = 0;

                while (dataID < _dataCount && rangeID <= _curRange)
                {
                    uint flushCount = 0; // Number of data elements to flush.

                    uint firstRangeID = rangeID;
                    uint firstDataID = dataID;

                    for (; rangeID <= _curRange; rangeID++)
                    {
                        range = ref Ranges[rangeID];

                        if (range.Type == RangeType.None || range.VertexCount == 0)
                            continue;

                        if (flushCount + range.VertexCount > FlushCapacity)
                            break;

                        flushCount += range.VertexCount;
                    }

                    uint rangeCount = rangeID - firstRangeID;
                    dataID += flushCount;

                    if (flushCount > 0)
                        FlushBuffer(cmd, camera, data, firstRangeID, rangeCount, firstDataID, flushCount);
                }
            }

            Reset();
        }

        private void FlushBuffer(GraphicsCommandQueue cmd, RenderCamera camera, ObjectRenderData data, uint rangeID, uint rangeCount, uint vertexStartIndex, uint vertexCount)
        {
            _buffer.GetStream(GraphicsPriority.Immediate, (buffer, stream) => stream.WriteRange(Data, vertexStartIndex, vertexCount));

            // Draw calls
            uint bufferOffset = 0;

            for (uint i = 0; i < rangeCount; i++)
            {
                ref SpriteRange range = ref Ranges[rangeID++];

                if (range.Type == RangeType.None || range.VertexCount == 0)
                    continue;

                HlslShader shader = range.Shader ?? _checkers[(int)range.Type](cmd, range, data);

                shader["spriteData"].Value = _buffer;
                shader["vertexOffset"].Value = bufferOffset;

                // Set common material properties
                if (range.Texture != null)
                {
                    if (range.Texture.IsMultisampled)
                    {
                        shader.Textures.DiffuseTextureMS.Value = range.Texture;
                        shader.Textures.SampleCount.Value = (uint)range.Texture.MultiSampleLevel;
                    }
                    else
                    {
                        shader.Textures.DiffuseTexture.Value = range.Texture;
                    }

                    Vector2F texSize = new Vector2F(range.Texture.Width, range.Texture.Height);
                    shader.SpriteBatch.TextureSize.Value = texSize;
                }

                cmd.SetScissorRectangles(range.Clip);

                shader.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
                cmd.Draw(shader, range.VertexCount);
                bufferOffset += range.VertexCount;
            }
        }

        private HlslShader CheckSpriteRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
        {
            if (range.Texture != null)
                return range.Texture.IsMultisampled ? _matDefaultMS : _matDefault;
            else
                return _matDefaultNoTexture;
        }

        private HlslShader CheckMsdfRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
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

        private HlslShader CheckLineRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
        {
            return _matLine;
        }

        private HlslShader CheckEllipseRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
        {
            return range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        private HlslShader CheckGridRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
        {
            return _matGrid; // range.Texture != null ? _matCircle : _matCircleNoTexture;
        }

        private HlslShader NoCheckRange(GraphicsCommandQueue cmd, SpriteRange range, ObjectRenderData data)
        {
            return null;
        }

        public void Dispose()
        {
            _matDefault.Dispose();
            _matDefaultNoTexture.Dispose();
            _matMsdf.Dispose();
            _matGrid.Dispose();
            _matLine.Dispose();
            _matCircle.Dispose();
            _matCircleNoTexture.Dispose();
            _buffer.Dispose();
        }

        /// <summary>
        /// Gets the maximum number of sprites that the current <see cref="SpriteBatcher"/> can render at a time when flushing.
        /// </summary>
        public uint FlushCapacity { get; }
    }
}
