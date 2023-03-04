namespace Molten.Graphics
{
    /// <summary>
    /// Indicates how the pipeline should interpret primitives for geometry or hull shaders. Shares value parity with D3D_PRIMITIVE from the DirectX SDK.
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_primitive</param>
    /// </summary>
    public enum GeometryHullTopology
    {
        Undefined = 0,

        Point = 1,

        Line = 2,

        Triangle = 3,

        LineAdj = 6,

        TriangleAdj = 7,

        Control1PointPatch = 8,

        Control2PointPatch = 9,

        Control3PointPatch = 10,

        Control4PointPatch = 11,

        Control5PointPatch = 12,

        Control6PointPatch = 13,

        Control7PointPatch = 14,

        Control8PointPatch = 0xF,

        Control9PointPatch = 0x10,

        Control10PointPatch = 17,

        Control11PointPatch = 18,

        Control12PointPatch = 19,

        Control13PointPatch = 20,

        Control14PointPatch = 21,

        Control15PointPatch = 22,

        Control16PointPatch = 23,

        Control17PointPatch = 24,

        Control18PointPatch = 25,

        Control19PointPatch = 26,

        Control20PointPatch = 27,

        Control21PointPatch = 28,

        Control22PointPatch = 29,

        Control23PointPatch = 30,

        Control24PointPatch = 0x1F,

        Control25PointPatch = 0x20,

        Control26PointPatch = 33,

        Control27PointPatch = 34,

        Control28PointPatch = 35,

        Control29PointPatch = 36,

        Control30PointPatch = 37,

        Control31PointPatch = 38,

        Control32PointPatch = 39,
    }
}
