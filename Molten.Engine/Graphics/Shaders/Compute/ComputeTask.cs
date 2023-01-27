namespace Molten.Graphics
{
    public class ComputeTask : HlslShader
    {
        public RWVariable[] UAVs;
        public ShaderComposition Composition;

        public ComputeTask(GraphicsDevice device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = device.CreateShaderComposition(ShaderType.Compute, this);
        }

        public override void GraphicsRelease()
        {
            Composition.Dispose();
            base.OnDispose();
        }
    }
}
