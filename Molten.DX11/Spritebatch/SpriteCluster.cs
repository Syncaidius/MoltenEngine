using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SpriteCluster
    {
        public ITexture2D texture;
        public SpriteVertex[] sprites;
        public int spriteCount;

        /// <summary>Number of sprites that have been drawn during the current draw call.</summary>
        public int drawnTo;
        public int drawnFrom;
        public int startVertex;

        internal SpriteCluster(int spriteCapacity)
        {
            sprites = new SpriteVertex[spriteCapacity];
        }
    }
}
