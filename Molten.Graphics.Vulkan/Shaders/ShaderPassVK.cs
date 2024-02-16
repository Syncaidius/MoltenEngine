namespace Molten.Graphics.Vulkan;

internal unsafe class ShaderPassVK : ShaderPass
{
    DescriptorPoolVK _descPool;
    DescriptorSetVK _descSet;

    internal ShaderPassVK(Shader material, string name = null) :
        base(material, name)
    { }

    protected override void OnInitialize(ShaderPassParameters parameters)
    {
        DeviceVK device = Device as DeviceVK;
        DescriptorLayout = new DescriptorSetLayoutVK(device, this);
        State = new PipelineStateVK(device, this, ref parameters);

        _descPool = new DescriptorPoolVK(device, DescriptorLayout, 5);
        _descSet = _descPool.Allocate(this, DescriptorLayout);
    }

    protected override void OnGraphicsRelease()
    {
        _descSet?.Dispose();    
        _descPool?.Dispose();
        DescriptorLayout?.Dispose();
        State?.Dispose();

        base.OnGraphicsRelease();
    }

    internal PipelineStateVK State { get; private set; }

    internal DescriptorSetLayoutVK DescriptorLayout { get; private set; }

    internal DescriptorSetVK DescriptorSet => _descSet;
}
