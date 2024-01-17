using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12PipelineState* _state;
    GraphicsPipelineStateDesc _desc;

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="pass"></param>
    /// <param name="parameters"></param>
    /// <param name="isTemplate">If true, the pipeline state will not be created on its parent <see cref="DeviceDX12"/>.</param>
    public PipelineStateDX12(DeviceDX12 device, ShaderPassDX12 pass, ref ShaderPassParameters parameters, bool isTemplate) :
        base(device)
    {
        _stateRasterizer = new RasterizerStateDX12(device, ref parameters);
        Device.Cache.Object<RasterizerStateDX12, RasterizerDesc>(ref _stateRasterizer);

        _stateBlend = new BlendStateDX12(device, ref parameters);
        Device.Cache.Object<BlendStateDX12, BlendStateDX12.CombinedDesc>(ref _stateBlend);

        _stateDepth = new DepthStateDX12(device, ref parameters);
        Device.Cache.Object<DepthStateDX12, DepthStateDX12.CombinedDesc>(ref _stateDepth);


        _desc = new GraphicsPipelineStateDesc()
        {
            RasterizerState = _stateRasterizer.Desc,
            BlendState = _stateBlend.Description.Desc,
            DepthStencilState = _stateDepth.Description.Desc,
            SampleMask = _stateBlend.Description.BlendSampleMask,
            Flags = PipelineStateFlags.None,            
        };

        IsTemplate = isTemplate;
        if (!IsTemplate)
        {
            Guid guid = ID3D12PipelineState.Guid;
            void* ptr = null;
            device.Ptr->CreateGraphicsPipelineState(_desc, &guid, &ptr);
            _state = (ID3D12PipelineState*)ptr;
        }
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _state);
    }

    internal bool IsTemplate { get; }

}
