namespace Molten.Graphics
{
    internal static class StateInterop
    {
        public static Silk.NET.Vulkan.PrimitiveTopology ToApi(this PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.Point:  return Silk.NET.Vulkan.PrimitiveTopology.PointList;
                case PrimitiveTopology.Line: return Silk.NET.Vulkan.PrimitiveTopology.LineList;
                case PrimitiveTopology.LineStrip: return Silk.NET.Vulkan.PrimitiveTopology.LineStrip;
                case PrimitiveTopology.Triangle: return Silk.NET.Vulkan.PrimitiveTopology.TriangleList;
                case PrimitiveTopology.TriangleStrip: return Silk.NET.Vulkan.PrimitiveTopology.TriangleStrip;
                case PrimitiveTopology.LineListWithAdjacency: return Silk.NET.Vulkan.PrimitiveTopology.LineListWithAdjacency;
                case PrimitiveTopology.LineStripWithAdjacency: return Silk.NET.Vulkan.PrimitiveTopology.LineStripWithAdjacency;
                case PrimitiveTopology.TriangleListWithAdjacency: return Silk.NET.Vulkan.PrimitiveTopology.TriangleListWithAdjacency;
                case PrimitiveTopology.TriangleStripWithAdjacency: return Silk.NET.Vulkan.PrimitiveTopology.TriangleStripWithAdjacency;         
            }

            if (topology >= PrimitiveTopology.PatchListWith1ControlPoint || topology <= PrimitiveTopology.PatchListWith32ControlPoints)
                return Silk.NET.Vulkan.PrimitiveTopology.PatchList;

            return Silk.NET.Vulkan.PrimitiveTopology.TriangleList;
        }
    }
}
