using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    /// <summary>
    /// Based on the D3D shader input flags enum: D3D_SHADER_INPUT_FLAGS.
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_input_flags</param>
    /// </summary>
    public enum ShaderInputFlags
    {
        None = 0x0,

        Userpacked = 0x1,

        ComparisonSampler = 0x2,

        TextureComponent0 = 0x4,

        TextureComponent1 = 0x8,

        TextureComponents = 0xC,

        Unused = 0x10,

        ForceDword = int.MaxValue
    }
}
