using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12PipelineState* _state;
    GraphicsPipelineStateDesc _desc;

    public PipelineStateDX12(DeviceDX12 device, ShaderPassDX12 pass, ref ShaderPassParameters parameters) :
        base(device)
    {
        _desc = new GraphicsPipelineStateDesc()
        {
            // TODO Retrieve input layout from device vertex layout cache.
        };

        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        device.Ptr->CreateGraphicsPipelineState(_desc, &guid, &ptr);
        _state = (ID3D12PipelineState*)ptr;
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _state);
    }

}
