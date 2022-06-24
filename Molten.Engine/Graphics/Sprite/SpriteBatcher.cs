using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract partial class SpriteBatcher : IDisposable
    {
        protected delegate void FlushRangeCallback(uint rangeCount, uint numVerticesInBuffer);

        protected struct SpriteRange
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

        static Vector2F DEFAULT_ORIGIN_CENTER = new Vector2F(0.5f);

        protected enum SpriteFormat
        {
            Sprite = 0, // Textured or untextured (rectangle) sprites

            MSDF = 1, // Multi-channel signed-distance field.

            Line = 2, // Untextured lines

            Triangle = 3, // Untextured triangles

            Ellipse = 4, // Untextured circles - Uses a geometry shader to handle this

            Grid = 5,
        }

        protected struct SpriteItem
        {
            public SpriteGpuData Vertex;
            public SpriteFormat Format;
            public ITexture2D Texture;
            public IMaterial Material;
            public int ClipID;
        }

        protected Rectangle[] Clips;
        protected SpriteItem[] Sprites;
        protected SpriteRange[] Ranges;
        protected SpriteGpuData[] Vertices;
        protected uint NextID;


        int _curClipID;
        SpriteStyle _style;
        uint _curRange;

        public SpriteBatcher(uint capacity)
        {
            Capacity = capacity;
            Vertices = new SpriteGpuData[capacity];
            Sprites = new SpriteItem[capacity];
            Ranges = new SpriteRange[capacity]; // Worst-case, we can expect the number of ranges to equal the capacity.

            Clips = new Rectangle[256];
            _style = new SpriteStyle()
            {
                PrimaryColor = Color.White,
                SecondaryColor = Color.White,
                Thickness = 0
            };
        }

        protected ref SpriteItem GetItem()
        {
            if (NextID == Sprites.Length) // Increase length by 50%
                Array.Resize(ref Sprites, Sprites.Length + (Sprites.Length / 2));

            Sprites[NextID].ClipID = _curClipID;
            return ref Sprites[NextID++];
        }

        /// <summary>
        /// Pushes a new clipping <see cref="Rectangle"/> into the current <see cref="SpriteBatcher"/>.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>Returns true if the clip was valid. 
        /// False will be returned if <paramref name="bounds"/> is invalid, or if the clip is outside of a previously-pushed clip.</returns>
        public bool PushClip(Rectangle bounds)
        {
            if (_curClipID == Clips.Length)
                Array.Resize(ref Clips, Clips.Length * 2);

            // Cull the bounds to the current clip, if any.
            if (_curClipID > 0)
            {
                Rectangle cur = Clips[_curClipID];

                bounds.X = MathHelper.Clamp(bounds.X, cur.X, cur.Right);
                bounds.Y = MathHelper.Clamp(bounds.Y, cur.Y, cur.Bottom);
                bounds.Right = MathHelper.Clamp(bounds.Right, cur.X, cur.Right);
                bounds.Bottom = MathHelper.Clamp(bounds.Bottom, cur.Y, cur.Bottom);
            }

            if (bounds.Width > 0 && bounds.Height > 0)
            {
                Clips[++_curClipID] = bounds;
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

        /// <summary>
        /// Sets the current <see cref="SpriteStyle"/> to use when drawing sprites.
        /// </summary>
        /// <param name="style"></param>
        public void SetStyle(ref SpriteStyle style)
        {
            _style = style;
        }

        public SpriteStyle GetStyle()
        {
            return _style;
        }

        /// <summary>
        /// Clears the current style back to <see cref="SpriteStyle.Default"/>.
        /// </summary>
        public void ClearStyle()
        {
            _style = SpriteStyle.Default;
        }

        /// <summary>
        /// Sets the current <see cref="SpriteStyle"/> to the specified color and thickness. Both the primary and secondary colors will be set to the same value.
        /// </summary>
        /// <param name="primaryColor"></param>
        /// <param name="thickness"></param>
        public void SetStyle(Color primaryColor, float thickness = 0)
        {
            _style.PrimaryColor = primaryColor;
            _style.SecondaryColor = primaryColor;
            _style.Thickness = thickness;
        }

        /// <summary>
        /// Sets the current <see cref="SpriteStyle"/> to use when drawing sprites.
        /// </summary>
        /// <param name="primaryColor"></param>
        /// <param name="secondaryColor"></param>
        /// <param name="thickness"></param>
        public void SetStyle(Color primaryColor, Color secondaryColor, float thickness)
        {
            _style.PrimaryColor = primaryColor;
            _style.SecondaryColor = secondaryColor;
            _style.Thickness = thickness;
        }

        public void DrawGrid(RectangleF bounds, Vector2F cellSize, float rotation, Vector2F origin, ITexture2D cellTexture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF source = cellTexture != null ? new RectangleF(0, 0, cellTexture.Width, cellTexture.Height) : RectangleF.Empty;
            ref SpriteItem item = ref DrawInternal(cellTexture,
                source,
                bounds.TopLeft,
                bounds.Size,
                rotation,
                origin,
                material,
                SpriteFormat.Grid,
                arraySlice);

            float cellIncX = bounds.Size.X / cellSize.X;
            float cellIncY = bounds.Size.Y / cellSize.Y;
            item.Vertex.Data.D1 = cellIncX / bounds.Size.X;
            item.Vertex.Data.D2 = cellIncY / bounds.Size.Y;
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void Draw(RectangleF source, RectangleF destination, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            DrawInternal(texture,
                source,
                destination.TopLeft,
                destination.Size,
                0,
                Vector2F.Zero,
                material,
                SpriteFormat.Sprite,
                arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="material"></param>
        public void Draw(RectangleF destination, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawInternal(texture, src, destination.TopLeft, destination.Size, 0, Vector2F.Zero, material, SpriteFormat.Sprite, arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(RectangleF destination, float rotation, Vector2F origin, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawInternal(texture, src, destination.TopLeft, destination.Size, rotation, origin, material, SpriteFormat.Sprite, arraySlice);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void Draw(Vector2F position, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawInternal(texture, src, position, new Vector2F(src.Width, src.Height), 0, Vector2F.Zero, material, SpriteFormat.Sprite, arraySlice);
        }

        public void Draw(Sprite sprite)
        {
            ref SpriteItem item = ref DrawInternal(sprite.Data.Texture,
                sprite.Data.Source,
                sprite.Position,
                sprite.Data.Source.Size * sprite.Scale,
                sprite.Rotation,
                sprite.Origin,
                sprite.Material,
                SpriteFormat.Sprite,
                sprite.Data.ArraySlice);

            item.Vertex.Color = sprite.Style.PrimaryColor;
            item.Vertex.Color2 = sprite.Style.SecondaryColor;
            item.Vertex.Data.Thickness = new Vector2F(sprite.Style.Thickness);
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
        public void Draw(Vector2F position, float rotation, Vector2F origin, ITexture2D texture, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF src = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawInternal(texture, src, position, new Vector2F(src.Width, src.Height), rotation, origin, material, SpriteFormat.Sprite, arraySlice);
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
        public void Draw(ITexture2D texture,
            RectangleF source,
            Vector2F position,
            Vector2F size,
            float rotation,
            Vector2F origin,
            IMaterial material,
            float arraySlice)
        {
            DrawInternal(texture, source, position, size, rotation, origin, material, SpriteFormat.Sprite, arraySlice);
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
        protected unsafe ref SpriteItem DrawInternal(ITexture2D texture,
            RectangleF source,
            Vector2F position,
            Vector2F size,
            float rotation,
            Vector2F origin,
            IMaterial material,
            SpriteFormat format,
            float arraySlice)
        {
            ref SpriteItem item = ref GetItem();
            item.Texture = texture;
            item.Material = material;
            item.Format = format;

            item.Vertex.Position = position;
            item.Vertex.Rotation = rotation;
            item.Vertex.ArraySlice = arraySlice;
            item.Vertex.Size = size;
            item.Vertex.Color = _style.PrimaryColor;
            item.Vertex.Color2 = _style.SecondaryColor;
            item.Vertex.Origin = origin;
            item.Vertex.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.
            item.Vertex.Data.Thickness = new Vector2F(_style.Thickness) / size; // Convert to UV coordinate system (0 - 1) range

            return ref item;
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
                uint start = i;

                _curRange = 0;
                range = ref Ranges[_curRange];

                ref SpriteItem item = ref Sprites[i];
                range.Format = item.Format;
                range.Texture = item.Texture;
                range.Material = item.Material;
                range.ClipID = item.ClipID;

                uint v = 0;
                for (; i < end; i++)
                {
                    item = ref Sprites[i];
                    Vertices[v++] = item.Vertex;

                    // If the current item does not match that of the current range, start a new range.
                    if (item.Texture != range.Texture ||
                        item.Material != range.Material ||
                        item.Format != range.Format ||
                        item.ClipID != range.ClipID)
                    {
                        range.VertexCount = i - start;
                        _curRange++;

                        range = ref Ranges[_curRange];
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
                    flushCallback(_curRange, v);
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
