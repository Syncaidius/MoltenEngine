using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract partial class SpriteBatcher : IDisposable
    {
        protected delegate void FlushRangeCallback(uint firstRangeID, uint rangeCount, uint dataStartIndex, uint numDataInBuffer);

        protected struct SpriteRange
        {
            public uint VertexCount;        // 4-bytes
            public ITexture2D Texture;      // 8-bytes (64-bit reference)
            public IMaterial Material;      // 8-bytes (64-bit reference)
            public Rectangle Clip;          // Clipping rectangle.
            public RangeType Type;           // 1-byte

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
            /// Untextured triangles.
            /// </summary>
            Triangle = 4,

            /// <summary>
            /// Ellipse or circular shape. It can be textured or untextured.
            /// </summary>
            Ellipse = 5,

            /// <summary>
            /// Textured or untextured grid.
            /// </summary>
            Grid = 6,
        }

        static Vector2F DEFAULT_ORIGIN_CENTER = new Vector2F(0.5f);

        protected Rectangle[] ClipStack;
        protected GpuData[] Data;
        protected SpriteRange[] Ranges;

        ushort _curClipID;
        uint _curRange;
        protected uint DataCount;

        /// <summary>
        /// Placeholder for internal rectangle/sprite styling.
        /// </summary>
        RectStyle _rectStyle;

        public SpriteBatcher(uint capacity)
        {
            _rectStyle = RectStyle.Default;

            FlushCapacity = capacity;
            Data = new GpuData[capacity];
            Ranges = new SpriteRange[capacity]; // Worst-case, we can expect the number of ranges to equal the capacity.

            ClipStack = new Rectangle[256];
            Reset();
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

                bounds.X = MathHelper.Clamp(bounds.X, cur.X, cur.Right);
                bounds.Y = MathHelper.Clamp(bounds.Y, cur.Y, cur.Bottom);
                bounds.Right = MathHelper.Clamp(bounds.Right, cur.X, cur.Right);
                bounds.Bottom = MathHelper.Clamp(bounds.Bottom, cur.Y, cur.Bottom);
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
        protected ref GpuData GetData(RangeType type, ITexture2D texture, IMaterial material)
        {
            ref SpriteRange range = ref Ranges[_curRange];
            if (range.Type != type ||
                range.Texture != texture ||
                range.Material != material ||
                range.VertexCount == FlushCapacity)
            {
                ref Rectangle curClip = ref range.Clip;
                range = ref GetRange(type);
                range.Texture = texture;
                range.Material = material;
                range.Clip = curClip;
            }

            range.VertexCount++;

            if (DataCount == Data.Length) // Increase length by 50%
                Array.Resize(ref Data, Data.Length + (Data.Length / 2));

            return ref Data[DataCount++];
        }

        /// <summary>Inserts a new sprite range, without any <see cref="GpuData"/>. 
        /// This is useful for state changes, which do not require any sprites to be drawn. e.g. pushing or popping clip rectangles.</summary>
        /// <param name="type"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref SpriteRange GetRange(RangeType type)
        {
            if(_curRange == Ranges.Length) // Increase length by 50%
                Array.Resize(ref Ranges, Ranges.Length + (Ranges.Length / 2));

            ref SpriteRange range = ref Ranges[++_curRange];
            range.Type = type;
            range.VertexCount = 0;

            return ref range;
        }

        private void Reset()
        {
            _curRange = 0;
            _curClipID = 0;
            DataCount = 0;
            Ranges[_curRange].Type = RangeType.None;
            Ranges[_curRange].Clip = ClipStack[_curClipID];
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, Color lineColor, float lineThickness, 
            ITexture2D cellTexture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = new Vector2F(lineThickness),
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, material, arraySlice, surfaceSlice);
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, 
            Color lineColor, Vector2F lineThickness, ITexture2D cellTexture = null, IMaterial material = null, 
            uint arraySlice = 0, uint surfaceSlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = lineThickness,
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, material, arraySlice, surfaceSlice);
        }

        public unsafe void DrawGrid(RectangleF bounds, 
            Vector2F cellSize, 
            float rotation,
            Vector2F origin, 
            ref GridStyle style, 
            ITexture2D cellTexture = null, 
            IMaterial material = null, 
            uint arraySlice = 0, 
            uint surfaceSlice = 0)
        {
            RectangleF source = cellTexture != null ? new RectangleF(0, 0, cellTexture.Width, cellTexture.Height) : RectangleF.Empty;
            float cellIncX = bounds.Size.X / cellSize.X;
            float cellIncY = bounds.Size.Y / cellSize.Y;

            ref GpuData data = ref GetData(RangeType.Grid, cellTexture, material);
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
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color">Sets the color of the sprite. This overrides <see cref="SpriteStyle.PrimaryColor"/> of the active <see cref="SpriteStyle"/>.</param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF destination, Color color, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
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
                material,
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
        public void Draw(RectangleF source, RectangleF destination, Color color, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
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
                material,
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
        public void Draw(RectangleF source, RectangleF destination, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            Draw(texture,
                source,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                ref style,
                material,
                arraySlice,
                surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="destination"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF destination, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, 0, Vector2F.Zero, ref style, material, arraySlice, surfaceSlice);
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
            IMaterial material = null, 
            uint arraySlice = 0, 
            uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, rotation, origin, ref style, material, arraySlice, surfaceSlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        /// <param name="surfaceSlice"></param>
        public void Draw(Vector2F position, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), 0, Vector2F.Zero, ref style, material, arraySlice, surfaceSlice);
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
                sprite.Material,
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
        public void Draw(Vector2F position, float rotation, Vector2F origin, ITexture2D texture, ref RectStyle style, IMaterial material = null, float arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), rotation, origin, ref style, material, arraySlice, surfaceSlice);
        }

        /// <summary>
        /// Adds a sprite to the batch using 2D coordinates.
        /// </summary>>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="size">The width and height of the sprite..</param>
        /// <param name="color"></param>
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
            IMaterial material,
            float arraySlice, uint surfaceSlice)
        {
            ref GpuData vertex = ref GetData(RangeType.Sprite, texture, material);
            vertex.Position = position;
            vertex.Rotation = rotation;
            vertex.Array.SrcArraySlice = arraySlice;
            vertex.Array.DestSurfaceSlice = surfaceSlice;
            vertex.Size = size;
            vertex.Color1 = style.FillColor;
            vertex.Color2 = style.BorderColor;
            vertex.Origin = origin;
            vertex.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.

            vertex.Extra.D1 = style.BorderThickness.Left / size.X; // Convert to UV coordinate system (0 - 1) range
            vertex.Extra.D2 = style.BorderThickness.Top / size.Y; // Convert to UV coordinate system (0 - 1) range
            vertex.Extra.D3 = style.BorderThickness.Right / size.X; // Convert to UV coordinate system (0 - 1) range
            vertex.Extra.D4 = style.BorderThickness.Bottom / size.Y; // Convert to UV coordinate system (0 - 1) range
        }

        protected void ProcessBatches(RenderCamera camera, FlushRangeCallback flushCallback)
        {
            ClipStack[0] = (Rectangle)camera.OutputSurface.Viewport.Bounds;

            SpriteRange t = new SpriteRange();
            ref SpriteRange range = ref t;

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.            
            uint dataID = 0;
            uint rangeID = 0;

            while (dataID < DataCount && rangeID <= _curRange)
            {
                uint flushCount = 0; // Number of data elements to flush.

                uint firstRangeID = rangeID;
                uint firstDataID = dataID;

                for(; rangeID <= _curRange; rangeID++)
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
                    flushCallback(firstRangeID, rangeCount, firstDataID, flushCount);
            }

            Reset();
        }

        public abstract void Dispose();

        /// <summary>
        /// Gets the maximum number of sprites that the current <see cref="SpriteBatcher"/> can render at a time when flushing.
        /// </summary>
        public uint FlushCapacity { get; }
    }
}
