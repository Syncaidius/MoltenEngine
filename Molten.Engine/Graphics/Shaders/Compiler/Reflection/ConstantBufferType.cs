using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    /// <summary>
    /// Shares value parity with the DirectX D3D_CBUFFER_TYPE enum.
    /// <para>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_cbuffer_type</para>
    /// </summary>
    public enum ConstantBufferType
    {
        None = 0x0,

        CBuffer = 0x0,

        TBuffer = 0x1,

        InterfacePointers = 0x2,

        ResourceBindInfo = 0x3,
    }
}
