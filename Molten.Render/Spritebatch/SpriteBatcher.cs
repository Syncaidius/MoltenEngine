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
        protected class Spriteitem
        {
            public SpriteVertex2 Vertex;
            public ITexture2D Texture;
            public IMaterial Material;
        }

        Spriteitem[] _sprites;
        int _nextID;

        public SpriteBatcher(int initialCapacity = 3000)
        {
            _sprites = new Spriteitem[initialCapacity];
        }

        protected Spriteitem GetItem()
        {
            if (_nextID == _sprites.Length) // Increase length by 50%
                Array.Resize(ref _sprites, _sprites.Length + (_sprites.Length / 2));

            _sprites[_nextID] = _sprites[_nextID] ?? new Spriteitem();
            return _sprites[_nextID++];
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
        public void Draw(ITexture2D texture, Rectangle source, Vector2F position, Vector2F scale, Color color, float rotation, Vector2F origin, IMaterial material, float depth = 0.0f)
        {
            Spriteitem item = GetItem();
            item.Texture = texture;
            item.Material = material;

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
        public void Draw(ITexture2D texture, Rectangle source, Vector3F position, Vector2F size, Color color, Vector3F rotation, Vector2F origin, IMaterial material)
        {
            Spriteitem item = GetItem();
            item.Texture = texture;
            item.Material = material;

            item.Vertex.Position = position;
            item.Vertex.Rotation = rotation;
            item.Vertex.Size = size;
            item.Vertex.Color = color;
            item.Vertex.Origin = origin;
            item.Vertex.UV = new Vector4F(source.X, source.Y, source.Right, source.Bottom);
        }

        private void Sort(RenderCamera camera)
        {
            // TODO use unsafe code when sorting
            // Store spritebatch data in class instances
            // SEE: https://github.com/MonoGame/MonoGame/blob/6f34eb393aa0ac005888d74c5c4c6ab5615fdc8c/MonoGame.Framework/Graphics/SpriteBatcher.cs#L147

            Array.Sort(_sprites, 0, _nextID);

            /* PROBLEMS TO SOLVE:
             *  - List.Sort() = unstable sort (inconsistent Z-sorting), since ordering will chang eif Z-depth is equal.
             *  - List.Orderby() = stable sort, but returns a new IEnumerable instance (garbage generation).
             *  - for 3D sprites, there is no way to sort by depth. Need to calculate distance from camera.
             * 
             * 
             * 
             */
        }
    }
}
