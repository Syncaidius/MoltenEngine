using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

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

    internal static PrimitiveTopologyType ToApiToplogyType(this GeometryHullTopology topology)
    {
        switch (topology)
        {
            case GeometryHullTopology.Triangle:
            case GeometryHullTopology.TriangleAdjacency:
                return PrimitiveTopologyType.Triangle;

            case GeometryHullTopology.Line:
                return PrimitiveTopologyType.Line;

            case GeometryHullTopology.Point:
                return PrimitiveTopologyType.Point;
        }

        if (topology >= GeometryHullTopology.Control1PointPatch && topology <= GeometryHullTopology.Control32PointPatch)
            return PrimitiveTopologyType.Patch;

        return PrimitiveTopologyType.Undefined;
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

    public static D3DSrvDimension ToApi(this ShaderResourceDimension dimension)
    {
        return (D3DSrvDimension)dimension;
    }

    public static ShaderResourceDimension FromApi(this D3DSrvDimension dimension)
    {
        return (ShaderResourceDimension)dimension;
    }

    public static D3DShaderInputType ToApi(this ShaderInputType type)
    {
        return (D3DShaderInputType)type;
    }

    public static ShaderInputType FromApi(this D3DShaderInputType type)
    {
        return (ShaderInputType)type;
    }

    public static D3DResourceReturnType ToApi(this ShaderReturnType type)
    {
        return (D3DResourceReturnType)type;
    }

    public static ShaderReturnType FromApi(this D3DResourceReturnType type)
    {
        return (ShaderReturnType)type;
    }

    public static D3DShaderInputFlags ToApi(this ShaderInputFlags type)
    {
        return (D3DShaderInputFlags)type;
    }

    public static ShaderInputFlags FromApi(this D3DShaderInputFlags type)
    {
        return (ShaderInputFlags)type;
    }

    public static D3DCBufferType ToApi(this ConstantBufferType type)
    {
        return (D3DCBufferType)type;
    }

    public static ConstantBufferType FromApi(this D3DCBufferType type)
    {
        return (ConstantBufferType)type;
    }

    public static InputClassification ToApi(this VertexInputType type)
    {
        return (InputClassification)type;
    }

    public static VertexInputType FromApi(this InputClassification type)
    {
        return (VertexInputType)type;
    }

    public static D3DPrimitiveTopology ToApi(this PrimitiveTopology type)
    {
        return (D3DPrimitiveTopology)type;
    }

    public static PrimitiveTopologyType ToApiPrimitiveType(this PrimitiveTopology type)
    {
        switch (type)
        {
            case PrimitiveTopology.Triangle:
            case PrimitiveTopology.TriangleStrip:
                return PrimitiveTopologyType.Triangle;

            case PrimitiveTopology.Line:
            case PrimitiveTopology.LineStrip:
                return PrimitiveTopologyType.Line;

            case PrimitiveTopology.Point:
                return PrimitiveTopologyType.Point;

            case PrimitiveTopology.Undefined:
                return PrimitiveTopologyType.Undefined;

            default:
                return PrimitiveTopologyType.Patch;
        }
    }
}
