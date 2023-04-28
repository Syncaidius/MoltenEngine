using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Dimensionality of an image. Used by <see cref="SpirvOpCode.OpTypeImage"/>.
    /// <para>See: https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Dim</para>
    /// </summary>
    public enum SpirvDimensionality
    {
        Dim1D = 0,

        Dim2D = 1,

        Dim3D = 2,

        Cube = 3,

        Rect = 4,

        Buffer = 5,

        SubpassData = 6,

        TileImageDataEXT = 4173,

        Max = 0x7fffffff,
    }
}
