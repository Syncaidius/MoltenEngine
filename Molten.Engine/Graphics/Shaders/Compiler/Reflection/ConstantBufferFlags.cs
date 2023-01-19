using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    /// <summary>
    /// Shares value parity with the DirectX D3D_SHADER_CBUFFER_FLAGS enum.
    /// <para>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_cbuffer_flags</para>
    /// </summary>
    public enum ConstantBufferFlags
    {
        None = 0,

        UserPacked = 1,

        ForceDWord = 0x7fffffff
    }
}
