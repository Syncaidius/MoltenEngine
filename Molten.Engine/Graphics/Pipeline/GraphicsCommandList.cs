namespace Molten.Graphics;

public abstract class GraphicsCommandList : GraphicsObject
{
    protected GraphicsCommandList(GraphicsQueue queue) : 
        base(queue.Device)
    {
        
        Queue = queue;
    }

    public abstract void Free();

    public GraphicsQueue Queue { get; }

    public GraphicsFence Fence { get; set; }

    public uint BranchIndex { get; set; }

    public GraphicsCommandListFlags Flags { get; set; }

    public GraphicsCommandList Previous { get; internal set; }
}
