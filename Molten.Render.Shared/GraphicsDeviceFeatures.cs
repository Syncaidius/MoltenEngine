using System;
using System.Collections.Generic;
using System.Text;

namespace Molten.Graphics
{
    internal abstract class GraphicsDeviceFeatures
    {
        /// <summary>Gets the maximum size of a single texture dimension i.e 2048 would mean the max texture size is 2048x2048.</summary>
        internal int MaxTextureDimension { get; private protected set; }

        /// <summary>
        /// Gets the maximum number of render targets (draw buffers) a fragment shader can output to simultaneously.
        /// </summary>
        internal int SimultaneousRenderSurfaces { get; private protected set; }

        /// <summary>Gets the maximum size of a single cube map dimension i.e 2048 would mean the max map size is 2048x2048.</summary>
        internal int MaxCubeMapDimension { get; private protected set; }
    }
}
