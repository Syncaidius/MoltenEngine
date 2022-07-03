using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract partial class SpriteBatcher : IDisposable
    {
        protected delegate void FlushRangeCallback(uint rangeCount, uint vertexStartIndex, uint numVerticesInBuffer);

        [StructLayout(LayoutKind.Explicit)]
        protected struct SpriteRange
        {
            [FieldOffset(0)] public uint BufferOffset;      // 4-bytes
            [FieldOffset(4)] public uint VertexCount;       // 4-bytes
            [FieldOffset(8)] public ITexture2D Texture;     // 8-bytes (64-bit reference)
            [FieldOffset(16)] public IMaterial Material;    // 8-bytes (64-bit reference)

            [FieldOffset(24)] public ItemType Type;         // 1-byte
            [FieldOffset(25)] public ushort ClipID;         // 2-bytes
            [FieldOffset(27)] public byte Reserved1;        // 1-byte unused.

            [FieldOffset(24)] public int Hash;              // 4-bytes overlapping Format, ClipID and Padding to give us a comparison hash.

            public override string ToString()
            {
                return $"Range -- Vertices: {VertexCount} -- Format: {Type}";
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        protected struct SpriteItem
        {
            [FieldOffset(0)] public ITexture2D Texture;
            [FieldOffset(8)] public IMaterial Material;

            [FieldOffset(16)] public ItemType Type;         // 1-byte
            [FieldOffset(17)] public ushort ClipID;         // 2-bytes
            [FieldOffset(19)] public byte Reserved1;        // 1-byte unused.

            [FieldOffset(16)] public int Hash;              // 4-bytes overlapping Format, ClipID and Padding to give us a comparison hash.
        }

        protected enum ItemType : byte
        {
            Sprite = 0, // Textured or untextured (rectangle) sprites

            MSDF = 1, // Multi-channel signed-distance field.

            Line = 2, // Untextured lines

            Triangle = 3, // Untextured triangles

            Ellipse = 4, // Untextured circles - Uses a geometry shader to handle this

            Grid = 5,
        }

        static Vector2F DEFAULT_ORIGIN_CENTER = new Vector2F(0.5f);

        protected Rectangle[] ClipStack;
        protected GpuData[] Data;
        protected SpriteItem[] Sprites;
        protected SpriteRange[] Ranges;
        protected uint NextID;

        ushort _curClipID;
        uint _curRange;

        /// <summary>
        /// Placeholder for internal rectangle/sprite styling.
        /// </summary>
        RectStyle _rectStyle;

        public SpriteBatcher(uint capacity)
        {
            _rectStyle = RectStyle.Default;

            Capacity = capacity;
            Data = new GpuData[capacity];
            Sprites = new SpriteItem[capacity];
            Ranges = new SpriteRange[capacity]; // Worst-case, we can expect the number of ranges to equal the capacity.

            ClipStack = new Rectangle[256];
        }

        protected uint GetItemID()
        {
            if (NextID == Sprites.Length)
            {  // Increase length by 50%
                int len = Sprites.Length + (Sprites.Length / 2);
                Array.Resize(ref Sprites, len);
                Array.Resize(ref Data, len);
            }

            Sprites[NextID].ClipID = _curClipID;
            return NextID++;
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
                return true;
            }

            return false;
        }

        public void PopClip()
        {
            if (_curClipID == 0)
                throw new Exception("There are no clips available to pop");

            _curClipID--;
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, Color lineColor, float lineThickness, ITexture2D cellTexture = null, IMaterial material = null, uint arraySlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = new Vector2F(lineThickness),
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, material, arraySlice);
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, Color cellColor, Color lineColor, Vector2F lineThickness, ITexture2D cellTexture = null, IMaterial material = null, uint arraySlice = 0)
        {
            GridStyle style = new GridStyle()
            {
                CellColor = cellColor,
                LineColor = lineColor,
                LineThickness = lineThickness,
            };

            DrawGrid(bounds, cellSize, rotation, origin, ref style, cellTexture, material, arraySlice);
        }

        public unsafe void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, ref GridStyle style, ITexture2D cellTexture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF source = cellTexture != null ? new RectangleF(0, 0, cellTexture.Width, cellTexture.Height) : RectangleF.Empty;
            float cellIncX = bounds.Size.X / cellSize.X;
            float cellIncY = bounds.Size.Y / cellSize.Y;


            uint id = GetItemID();
            ref SpriteItem item = ref Sprites[id];
            item.Texture = cellTexture;
            item.Material = material;
            item.Type = ItemType.Grid;

            ref GpuData data = ref Data[id];
            data.Position = bounds.TopLeft;
            data.Rotation = rotation;
            data.Size = bounds.Size;
            data.Color1 = style.CellColor;
            data.Color2 = style.LineColor;
            data.Origin = origin;
            data.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.
            data.ArraySlice = arraySlice;

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
        public void Draw(RectangleF destination, Color color, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
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
                arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color">Sets the color of the sprite. This overrides <see cref="SpriteStyle.PrimaryColor"/> of the active <see cref="SpriteStyle"/>.</param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF source, RectangleF destination, Color color, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
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
                arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF source, RectangleF destination, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            Draw(texture,
                source,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                ref style,
                material,
                arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="material"></param>
        public void Draw(RectangleF destination, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, 0, Vector2F.Zero, ref style, material, arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(RectangleF destination, float rotation, Vector2F origin, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, destination.TopLeft, destination.Size, rotation, origin, ref style, material, arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void Draw(Vector2F position, ref RectStyle style, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), 0, Vector2F.Zero, ref style, material, arraySlice);
        }

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
                sprite.Data.ArraySlice);
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
        public void Draw(Vector2F position, float rotation, Vector2F origin, ITexture2D texture, ref RectStyle style, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            Draw(texture, src, position, new Vector2F(src.Width, src.Height), rotation, origin, ref style, material, arraySlice);
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
            float arraySlice)
        {
            uint id = GetItemID();
            ref SpriteItem item = ref Sprites[id];
            item.Texture = texture;
            item.Material = material;
            item.Type = ItemType.Sprite;

            ref GpuData vertex = ref Data[id];
            vertex.Position = position;
            vertex.Rotation = rotation;
            vertex.ArraySlice = arraySlice;
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

        protected void ProcessBatches(FlushRangeCallback flushCallback)
        {
            SpriteRange t = new SpriteRange();
            ref SpriteRange range = ref t;

            // Chop up the sprite list into ranges of vertices. Each range is equivilent to one draw call.            
            uint i = 0;
            while (i < NextID)
            {
                // Reset vertex array pointer and ranges, so we can prepare the next batch of vertices.
                uint remaining = NextID - i;
                uint end = i + Math.Min(remaining, Capacity);
                uint firstVertexID = i;
                uint start = i;

                _curRange = 0;
                range = ref Ranges[_curRange];

                ref SpriteItem item = ref Sprites[i];
                range.Type = item.Type;
                range.Texture = item.Texture;
                range.Material = item.Material;
                range.ClipID = item.ClipID;

                uint v = 0;
                for (; i < end; i++)
                {
                    item = ref Sprites[i];
                    v++;

                    // If the current item does not match that of the current range, start a new range.
                    if (item.Texture != range.Texture ||
                        item.Material != range.Material ||
                        item.Hash != range.Hash)
                    {
                        range.VertexCount = i - start;
                        _curRange++;

                        range = ref Ranges[_curRange];
                        start = i;
                        range.Texture = item.Texture;
                        range.Material = item.Material;
                        range.Hash = item.Hash;
                    }
                }

                // Include the last range, if it has any vertices.
                range.VertexCount = i - start;
                if (range.VertexCount > 0)
                    _curRange++;

                if (_curRange > 0)
                    flushCallback(_curRange, firstVertexID, v);
            }

            // Reset
            NextID = 0;
        }

        public abstract void Dispose();

        /// <summary>
        /// Gets the capacity of the Spritebatcher.
        /// </summary>
        public uint Capacity { get; }
    }
}
