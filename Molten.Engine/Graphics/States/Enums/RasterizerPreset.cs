using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents several rasterizer state presets.</summary>
    public enum RasterizerPreset
    {
        /// <summary>The default rasterizer state.</summary>
        Default = 0,

        /// <summary>The same as the default rasterizer state, but with wireframe enabled.</summary>
        Wireframe = 1,

        /// <summary>The same as the default rasterizer state, but with scissor testing enabled.</summary>
        ScissorTest = 2,

        /// <summary>Culling is disabled. Back and front faces will be drawn.</summary>
        NoCulling = 3,

        /// <summary>
        /// The same as <see cref="Default"/> but with multisampling enabled.
        /// </summary>
        DefaultMultisample = 4,

        /// <summary>
        /// The same as <see cref="ScissorTest"/> but with multisampling enabled.
        /// </summary>
        ScissorTestMultisample = 5,
    }
}
