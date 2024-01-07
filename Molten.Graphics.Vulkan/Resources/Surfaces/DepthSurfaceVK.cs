using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal class DepthSurfaceVK : Texture2DVK, IDepthStencilSurface
{
    internal DepthSurfaceVK(DeviceVK device, uint width, uint height, uint mipCount, uint arraySize,
        AntiAliasLevel aaLevel, 
        MSAAQuality sampleQuality, 
        DepthFormat format, 
        GraphicsResourceFlags flags, 
        string name) : 
        base(device, width, height, mipCount, arraySize, aaLevel, sampleQuality, format.ToGraphicsFormat(), flags, name)
    {
        DepthFormat = format;
        Viewport = new ViewportF(0, 0, Width, Height);
    }

    public void Clear(GraphicsPriority priority, DepthClearFlags flags, float depthValue = 1.0f, byte stencilValue = 0)
    {
        DepthClearTaskVK task = Device.Tasks.Get<DepthClearTaskVK>();
        task.DepthValue = depthValue;
        task.StencilValue = stencilValue;
        Device.Tasks.Push(priority, this, task);
    }

    public DepthFormat DepthFormat { get; }

    public ViewportF Viewport { get; }

    /// <summary>
    /// Gets surface clear color, if any.
    /// </summary>
    internal ClearDepthStencilValue? ClearValue { get; set; }
}
