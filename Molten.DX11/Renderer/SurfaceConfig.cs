using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SurfaceConfig
    {
        SurfaceSizeMode _mode;
        internal readonly RenderSurfaceBase Surface;

        internal SurfaceConfig(RenderSurfaceBase surface, SurfaceSizeMode mode = SurfaceSizeMode.Full) {
            Surface = surface;
            _mode = mode;
        }

        internal void RefreshSize(int minWidth, int minHeight)
        {
            switch (_mode)
            {
                case SurfaceSizeMode.Full:
                    Surface.Resize(minWidth, minHeight);
                    break;

                case SurfaceSizeMode.Half:
                    Surface.Resize((minWidth / 2) + 1, (minHeight / 2) + 1);
                    break;
            }
        }
    }

    public enum SurfaceSizeMode
    {
        /// <summary>
        /// The surface will be at least the width and height of the largest-rendered surface.
        /// </summary>
        Full = 0,

        /// <summary>
        /// The surface will be at least half the width and height of the largest-rendered surface.
        /// </summary>
        Half = 1,

        /// <summary>
        /// The surface will remain at a fixed size regardless of resolution changes.
        /// </summary>
        Fixe = 2,
    }
}
