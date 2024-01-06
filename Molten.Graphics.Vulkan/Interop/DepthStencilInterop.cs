using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal static class DepthStencilInterop
{
    public static ComparisonFunction FromApi(this CompareOp op)
    {
        switch (op)
        {
            case CompareOp.Never: return ComparisonFunction.Never;
            case CompareOp.Less: return ComparisonFunction.Less;
            case CompareOp.LessOrEqual: return ComparisonFunction.LessEqual;
            case CompareOp.Equal: return ComparisonFunction.Equal;
            case CompareOp.Greater: return ComparisonFunction.Greater;
            case CompareOp.GreaterOrEqual: return ComparisonFunction.GreaterEqual;

            default:
            case CompareOp.Always: return ComparisonFunction.Always;
        }
    }

    public static CompareOp ToApi(this ComparisonFunction op)
    {
        switch (op)
        {
            case ComparisonFunction.Never: return CompareOp.Never;
            case ComparisonFunction.Less: return CompareOp.Less;
            case ComparisonFunction.LessEqual: return CompareOp.LessOrEqual;
            case ComparisonFunction.Equal: return CompareOp.Equal;
            case ComparisonFunction.Greater: return CompareOp.Greater;
            case ComparisonFunction.GreaterEqual: return CompareOp.GreaterOrEqual;

            default:
            case ComparisonFunction.Always: return CompareOp.Always;
        }
    }

    public static DepthStencilOperation FromApi(this StencilOp op)
    {
        switch (op)
        {
            case StencilOp.IncrementAndWrap: return DepthStencilOperation.IncrementAndWrap;
            case StencilOp.DecrementAndWrap: return DepthStencilOperation.DecrementAndWrap;
            case StencilOp.Replace: return DepthStencilOperation.Replace;
            case StencilOp.IncrementAndClamp: return DepthStencilOperation.IncrementAndClamp;
            case StencilOp.DecrementAndClamp: return DepthStencilOperation.DecrementAndClamp;
            case StencilOp.Invert: return DepthStencilOperation.Invert;
            case StencilOp.Zero: return DepthStencilOperation.Zero;

            default:
            case StencilOp.Keep: return DepthStencilOperation.Keep;
        }
    }

    public static StencilOp ToApi(this DepthStencilOperation op)
    {
        switch (op)
        {
            case DepthStencilOperation.IncrementAndWrap: return StencilOp.IncrementAndWrap;
            case DepthStencilOperation.DecrementAndWrap: return StencilOp.DecrementAndWrap;
            case DepthStencilOperation.Replace: return StencilOp.Replace;
            case DepthStencilOperation.IncrementAndClamp: return StencilOp.IncrementAndClamp;
            case DepthStencilOperation.DecrementAndClamp: return StencilOp.DecrementAndClamp;
            case DepthStencilOperation.Invert: return StencilOp.Invert;
            case DepthStencilOperation.Zero: return StencilOp.Zero;

            default:
            case DepthStencilOperation.Keep: return StencilOp.Keep;
        }
    }
}
