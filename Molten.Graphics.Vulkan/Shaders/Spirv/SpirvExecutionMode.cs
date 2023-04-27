using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// See: https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Execution_Mode
    /// </summary>
    public enum SpirvExecutionMode
    {
        Invocations = 0,

        SpacingEqual = 1,

        SpacingFractionalEven = 2,

        SpacingFractionalOdd = 3,

        VertexOrderCw = 4,

        VertexOrderCcw = 5,

        PixelCenterInteger = 6,

        OriginUpperLeft = 7,

        OriginLowerLeft = 8,

        EarlyFragmentTests = 9,

        PointMode = 10,

        Xfb = 11,

        DepthReplacing = 12,

        DepthGreater = 14,

        DepthLess = 15,

        DepthUnchanged = 16,

        LocalSize = 17,

        LocalSizeHint = 18,

        InputPoints = 19,

        InputLines = 20,

        InputLinesAdjacency = 21,

        Triangles = 22,

        InputTrianglesAdjacency = 23,

        Quads = 24,

        Isolines = 25,

        OutputVertices = 26,

        OutputPoints = 27,

        OutputLineStrip = 28,

        OutputTriangleStrip = 29,

        VecTypeHint = 30,

        ContractionOff = 31,

        PostDepthCoverage = 4446,

        StencilRefReplacingEXT = 5027,

        Max = 0x7fffffff,
    }
}
