using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents the type of shader system value (SV_) type. e.g. SV_VertexID or SV_InstanceID.
    /// <para>Replicates the values of the DirectX type: D3D_NAME. See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_name</para>
    /// </summary>
    public enum ShaderSVType
    {
        Undefined = 0,

        Position = 1,

        ClipDistance = 2,

        CullDistance = 3,

        RenderTargetArrayIndex = 4,

        ViewportArrayIndex = 5,

        VertexID = 6,

        PrimitiveID = 7,

        InstanceID = 8,

        IsFrontFace = 9,

        SampleIndex = 10,

        FinalQuadEdgeTessfactor = 11,

        FinalQuadInsideTessfactor = 12,

        FinalTriEdgeTessfactor = 13,

        FinalTriInsideTessfactor = 14,

        FinalLineDetailTessfactor = 0xF,

        FinalLineDensityTessfactor = 0x10,

        Barycentrics = 23,

        Shadingrate = 24,

        Cullprimitive = 25,

        Target = 0x40,

        Depth = 65,

        Coverage = 66,

        DepthGreaterEqual = 67,

        DepthLessEqual = 68,

        StencilRef = 69,

        InnerCoverage = 70
    }
}
