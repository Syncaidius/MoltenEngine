using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures.DDS
{
    [FlagsAttribute]
    /// <summary>DDS_HEADER : dwCaps. http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx. </summary>
    internal enum DDSCapabilities : uint
    {
        /// <summary>Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).</summary>
        Complex = 0x8,

        /// <summary>Optional; should be used for a mipmap.</summary>
        MipMap = 0x400000,

        /// <summary>Required</summary>
        Texture = 0x1000,
    }

    [FlagsAttribute]
    /// <summary>DDS_HEADER : dwCaps2. http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx. </summary>
    internal enum DDSAdditionalCapabilities : uint
    {
        /// <summary>No additional capabilities.</summary>
        None = 0x0,

        /// <summary>Required for a cube map.</summary>
        CubeMap = 0x200,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapPositiveX = 0x400,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapNegativeX = 0x800,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapPositiveY = 0x1000,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapNegativeY = 0x2000,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapPositiveZ = 0x4000,

        /// <summary>Required when these surfaces are stored in a cube map.</summary>
        CubeMapNegativeZ = 0x8000,

        /// <summary>Required for a volume texture.</summary>
        Volume = 0x200000,
    }
}
