namespace Molten.Graphics
{
    public class ComputeTask : HlslShader, IComputeTask
    {
        internal RWVariable[] UAVs;
        internal CSComposition Composition;

        internal ComputeTask(DeviceDX11 device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = new CSComposition(this, ShaderType.Compute);
        }

        internal override void PipelineRelease()
        {
            Composition.Dispose();
        }
    }
}
