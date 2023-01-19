using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Based on the D3D shader input type enum: D3D_RESOURCE_RETURN_TYPE.
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_resource_return_type</param>
    /// </summary>
    public enum ShaderReturnType
    {
        None = 0x0,

        UNorm = 0x1,

        SNorm = 0x2,

        SInt = 0x3,

        UInt = 0x4,

        Float = 0x5,

        Mixed = 0x6,

        Double = 0x7,

        Continued = 0x8,
    }
}
