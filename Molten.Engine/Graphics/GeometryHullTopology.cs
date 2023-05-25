namespace Molten.Graphics
{
    /// <summary>
    /// Indicates how the pipeline should interpret primitives for geometry or hull shaders. Shares value parity with D3D_PRIMITIVE from the DirectX SDK.
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_primitive</param>
    /// </summary>
    public enum GeometryHullTopology
    {
        /// <summary>
        /// Undefined topology.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Point topology.
        /// </summary>
        Point = 1,

        /// <summary>
        /// Line topology.
        /// </summary>
        Line = 2,

        /// <summary>
        /// Triangle topology.
        /// </summary>
        Triangle = 3,

        /// <summary>
        /// Line adjacency topology.
        /// </summary>
        LineAdjacency = 6,

        /// <summary>
        /// Triangle adjacency topology.
        /// </summary>
        TriangleAdjaccency = 7,

        /// <summary>
        /// A patch with 1 control point.
        /// </summary>
        Control1PointPatch = 8,

        /// <summary>
        /// A patch with 2 control points.
        /// </summary>
        Control2PointPatch = 9,

        /// <summary>
        /// A patch with 3 control points.
        /// </summary>
        Control3PointPatch = 10,

        /// <summary>
        /// A patch with 4 control points.
        /// </summary>
        Control4PointPatch = 11,

        /// <summary>
        /// A patch with 5 control points.
        /// </summary>
        Control5PointPatch = 12,

        /// <summary>
        /// A patch with 6 control points.
        /// </summary>
        Control6PointPatch = 13,

        /// <summary>
        /// A patch with 7 control points.
        /// </summary>
        Control7PointPatch = 14,

        /// <summary>
        /// A patch with 8 control points.
        /// </summary>
        Control8PointPatch = 0xF,

        /// <summary>
        /// A patch with 9 control points.
        /// </summary>
        Control9PointPatch = 0x10,

        /// <summary>
        /// A patch with 10 control points.
        /// </summary>
        Control10PointPatch = 17,

        /// <summary>
        /// A patch with 11 control points.
        /// </summary>
        Control11PointPatch = 18,

        /// <summary>
        /// A patch with 12 control points.
        /// </summary>
        Control12PointPatch = 19,

        /// <summary>
        /// A patch with 13 control points.
        /// </summary>
        Control13PointPatch = 20,

        /// <summary>
        /// A patch with 14 control points.
        /// </summary>
        Control14PointPatch = 21,

        /// <summary>
        /// A patch with 15 control points.
        /// </summary>
        Control15PointPatch = 22,

        /// <summary>
        /// A patch with 16 control points.
        /// </summary>
        Control16PointPatch = 23,

        /// <summary>
        /// A patch with 17 control points.
        /// </summary>
        Control17PointPatch = 24,

        /// <summary>
        /// A patch with 18 control points.
        /// </summary>
        Control18PointPatch = 25,

        /// <summary>
        /// A patch with 19 control points.
        /// </summary>
        Control19PointPatch = 26,

        /// <summary>
        /// A patch with 20 control points.
        /// </summary>
        Control20PointPatch = 27,

        /// <summary>
        /// A patch with 21 control points.
        /// </summary>
        Control21PointPatch = 28,

        /// <summary>
        /// A patch with 22 control points.
        /// </summary>
        Control22PointPatch = 29,

        /// <summary>
        /// A patch with 6 control points.
        /// </summary>
        Control23PointPatch = 30,

        /// <summary>
        /// A patch with 24 control points.
        /// </summary>
        Control24PointPatch = 0x1F,

        /// <summary>
        /// A patch with 25 control points.
        /// </summary>
        Control25PointPatch = 0x20,

        /// <summary>
        /// A patch with 26 control points.
        /// </summary>
        Control26PointPatch = 33,

        /// <summary>
        /// A patch with 27 control points.
        /// </summary>
        Control27PointPatch = 34,

        /// <summary>
        /// A patch with 28 control points.
        /// </summary>
        Control28PointPatch = 35,

        /// <summary>
        /// A patch with 29 control points.
        /// </summary>
        Control29PointPatch = 36,

        /// <summary>
        /// A patch with 30 control points.
        /// </summary>
        Control30PointPatch = 37,

        /// <summary>
        /// A patch with 31 control points.
        /// </summary>
        Control31PointPatch = 38,

        /// <summary>
        /// A patch with 32 control points.
        /// </summary>
        Control32PointPatch = 39,
    }
}
