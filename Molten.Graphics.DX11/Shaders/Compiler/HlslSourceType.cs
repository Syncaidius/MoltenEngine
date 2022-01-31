using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal enum HlslSourceType
    {
        /// <summary>
        /// The Hlsl source is from a standard file loaded via an asset manager.
        /// </summary>
        StandardFile = 0,

        /// <summary>
        /// The Hlsl source is from an embedded resource file.
        /// </summary>
        EmbeddedFile = 1
    }
}
