using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract class SpriteBatcher
    {
        protected class SpriteCluster
        {

        }

        protected class SpriteItem : IComparable<SpriteItem>
        {
            public SpriteVertex2 Vertex;
            public ITexture2D Texture;
            public IMaterial Material;
            public float ZKey;

            public int CompareTo(SpriteItem other)
            {
                if (ZKey > other.ZKey)
                    return 1;
                else if (ZKey < other.ZKey)
                    return -1;
                else
                    return 0;
            }
        }
       
        protected SpriteItem[] Sprites;
        protected int NextID;

        public SpriteBatcher(int initialCapacity)
        {
            Sprites = new SpriteItem[initialCapacity];
        }

        protected SpriteItem GetItem()
        {
            if (NextID == Sprites.Length) // Increase length by 50%
                Array.Resize(ref Sprites, Sprites.Length + (Sprites.Length / 2));

            Sprites[NextID] = Sprites[NextID] ?? new SpriteItem();
            return Sprites[NextID++];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material"></param>
        /// <param name="depth">The z-depth of the sprite.</param>
        public void Draw(ITexture2D texture, Rectangle source, Vector2F position, Vector2F scale, Color color, float rotation, Vector2F origin, IMaterial material, float depth = 0.0f)
        {
            SpriteItem item = GetItem();
            item.Texture = texture;
            item.Material = material;
            item.ZKey = depth;

            item.Vertex.Position = new Vector3F(position, depth);
            item.Vertex.Rotation = new Vector3F(rotation, 0, 0);
            item.Vertex.Size = new Vector2F(source.X * scale.X, source.Y * scale.Y);
            item.Vertex.Color = color;
            item.Vertex.Origin = origin;
            item.Vertex.UV = new Vector4F(source.X, source.Y, source.Right, source.Bottom);
        }

        /// <summary>
        /// Adds a sprite to the batch
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material"></param>
        public void Draw(ITexture2D texture, Rectangle source, Vector3F position, Vector2F scale, Color color, Vector3F rotation, Vector2F origin, IMaterial material)
        {
            SpriteItem item = GetItem();
            item.Texture = texture;
            item.Material = material;

            item.Vertex.Position = position;
            item.Vertex.Rotation = rotation;
            item.Vertex.Size = new Vector2F(source.Width * scale.X, source.Height * scale.Y);
            item.Vertex.Color = color;
            item.Vertex.Origin = origin;
            item.Vertex.UV = new Vector4F(source.X, source.Y, source.Right, source.Bottom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="depthSort">If true, the Z-axis of sprites is used for sorting instead of their distance from the provided camera.</param>
        protected void Sort(RenderCamera camera, bool depthSort = true)
        {
            if (!depthSort)
            {
                Vector3F camPos = camera.Position;
                for (int i = 0; i < NextID; i++)
                    Vector3F.Distance(ref camPos, ref Sprites[i].Vertex.Position, out Sprites[i].ZKey);
            }

            Array.Sort(Sprites, 0, NextID);
        }
    }
}
