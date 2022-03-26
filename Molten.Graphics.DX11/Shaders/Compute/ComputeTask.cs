namespace Molten.Graphics
{
    public class ComputeTask : HlslShader, IComputeTask
    {
        internal RWVariable[] UAVs;
        internal CSComposition Composition;

        internal ComputeTask(Device device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = new CSComposition(this, false, ShaderType.Compute);
        }

        internal override void PipelineRelease()
        {
            Composition.Dispose();
        }
    }
}
