namespace Molten.Graphics
{
    public class ComputeTask : HlslShader<ComputePass>
    {
        public RWVariable[] UAVs;

        public ComputeTask(GraphicsDevice device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
        }
    }
}
