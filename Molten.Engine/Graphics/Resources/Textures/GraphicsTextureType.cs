using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsTextureType
    {
        /// <summary>
        /// Represents a 1D texture in graphics.
        /// </summary>
        Texture1D = 0,

        /// <summary>
        /// Represents a 2D texture.
        /// </summary>
        Texture2D = 1,

        /// <summary>
        /// Represents a 3D texture.
        /// </summary>
        Texture3D = 2,

        /// <summary>
        /// Represents a cube texture.
        /// </summary>
        TextureCube = 3,
    }
}
