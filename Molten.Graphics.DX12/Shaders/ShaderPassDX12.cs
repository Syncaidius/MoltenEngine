namespace Molten.Graphics.DX12;

public class ShaderPassDX12 : HlslPass
{
    public ShaderPassDX12(HlslShader parent, string name) : 
        base(parent, name) { }

    protected override void OnInitialize(ref ShaderPassParameters parameters)
    {
        DeviceDX12 device = Device as DeviceDX12;
        State = new PipelineStateDX12(device, this, ref parameters);
    }

    protected override void OnGraphicsRelease()
    {
        State?.Dispose();

        base.OnGraphicsRelease();
    }

    internal PipelineStateDX12 State { get; private set; }
}
