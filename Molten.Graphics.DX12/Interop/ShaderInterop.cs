using Silk.NET.Core.Native;

namespace Molten.Graphics.DX12;
internal static class ShaderInterop
{
    public static D3DPrimitiveTopology ToApi(this GeometryHullTopology topology)
    {
        switch (topology)
        {
            case GeometryHullTopology.Line:
                return D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist;
            case GeometryHullTopology.LineAdjacency:
                return D3DPrimitiveTopology.D3DPrimitiveTopologyLinelistAdj;
            case GeometryHullTopology.Point:
                return D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist;
            case GeometryHullTopology.Triangle:
                return D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist;
            case GeometryHullTopology.TriangleAdjacency:
                return D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestripAdj;
            default:
                {
                    // If the type is a control-pointed patch, translate patch type.
                    if (topology >= GeometryHullTopology.Control1PointPatch && 
                        topology <= GeometryHullTopology.Control32PointPatch)
                        return D3DPrimitiveTopology.D3DPrimitiveTopology1ControlPointPatchlist + (topology - GeometryHullTopology.Control1PointPatch);
                    else
                        return D3DPrimitiveTopology.D3DPrimitiveTopologyUndefined;
                }
        };
    }

    public static GeometryHullTopology FromApi(this D3DPrimitiveTopology topology)
    {
        switch (topology)
        {
            case D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist:
                return GeometryHullTopology.Line;
            case D3DPrimitiveTopology.D3DPrimitiveTopologyLinelistAdj:
                return GeometryHullTopology.LineAdjacency;
            case D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist:
                return GeometryHullTopology.Point;
            case D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist:
                return GeometryHullTopology.Triangle;
            case D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestripAdj:
                return GeometryHullTopology.TriangleAdjacency;
            default:
                {
                    // If the type is a control-pointed patch, translate patch type.
                    if (topology >= D3DPrimitiveTopology.D3DPrimitiveTopology1ControlPointPatchlist && 
                        topology <= D3DPrimitiveTopology.D3DPrimitiveTopology32ControlPointPatchlist)
                        return GeometryHullTopology.Control1PointPatch + (topology - D3DPrimitiveTopology.D3DPrimitiveTopology1ControlPointPatchlist);
                    else
                        return GeometryHullTopology.Undefined;
                }
        };
    }
}
