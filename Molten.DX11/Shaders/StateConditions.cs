using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum StateConditions : byte
    {
        /// <summary>
        /// No conditions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Multisampling is enabled.
        /// </summary>
        Multisampling = 1,

        /// <summary>
        /// Anisotropic filtering is enabled
        /// </summary>
        AnisotropicFiltering = 1 << 1,

        /// <summary>
        /// Scissor testing is enabled
        /// </summary>
        ScissorTest = 1 << 2,

        /// <summary>
        /// Skybox rendering is enabled.
        /// </summary>
        Skybox = 1 << 3,

        /// <summary>
        /// All conditions are present.
        /// </summary>
        All = Multisampling | AnisotropicFiltering | ScissorTest | Skybox
    }
}
