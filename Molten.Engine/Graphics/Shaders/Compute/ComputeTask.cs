namespace Molten.Graphics
{
    public class ComputeTask : HlslShader
    {
        public RWVariable[] UAVs;
        public ShaderComposition Composition;

        public unsafe void* InputByteCode { get; set; }

        public ComputeTask(GraphicsDevice device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = new ShaderComposition(this, ShaderType.Compute);
        }

        public override void GraphicsRelease()
        {
            Composition.Dispose();
            base.OnDispose();
        }
    }
}
