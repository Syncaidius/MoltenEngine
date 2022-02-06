using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum ShaderCompileFlags
    {
        None = 0,

        /// <summary>
        /// The Hlsl source is from an embedded resource file.
        /// </summary>
        EmbeddedFile = 1
    }
}
