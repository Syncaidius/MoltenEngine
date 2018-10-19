using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    /// <summary>
    /// A helper structure for providing the dimensions of a DDS compression block.
    /// </summary>
    internal struct BCDimensions
    {
        /// <summary>
        /// The block location, in blocks.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y block location, in blocks.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the block, in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the block, in pixels.
        /// </summary>
        public int Height;
    }
}
