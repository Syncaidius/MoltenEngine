using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>, IEquatable<PipelineStateDX12>
{
    ID3D12PipelineState* _handle;

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;
    PipelineInputLayoutDX12 _inputLayout;
    RootSignatureDX12 _rootSignature;

    public PipelineStateDX12(DeviceDX12 device, ID3D12PipelineState* handle) : 
        base(device)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PipelineStateDX12"/>.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="pass"></param>
    /// <param name="parameters"></param>
    /// <param name="isTemplate">If true, the pipeline state will not be created on its parent <see cref="DeviceDX12"/>.</param>
    public PipelineStateDX12(DeviceDX12 device, ShaderPassDX12 pass, bool isTemplate) :
        base(device)
    {
        _stateBlend = pass.BlendState;
        _stateDepth = pass.DepthState;
        _stateRasterizer = pass.RasterizerState;

        IsTemplate = isTemplate;
    }

    public bool Equals(PipelineStateDX12 other)
    {
        if(this == other)
            return true;

        if(_stateRasterizer != other._stateRasterizer
            || _stateBlend != other._stateBlend
            || _stateDepth != other._stateDepth)
            return false;

        // TODO
        //  - Compare the IDs of any cachable parts of the state (depth, blend, etc).
        //  - Compare the bound buffers, slots and their formats.
        //  - Compare the bound shaders.

        return true;
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
    }

    internal bool IsTemplate { get; }

    internal ID3D12PipelineState* Handle => _handle;
}
