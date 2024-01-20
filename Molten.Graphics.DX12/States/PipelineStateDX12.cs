using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>, IEquatable<PipelineStateDX12>
{
    ID3D12PipelineState* _state;
    GraphicsPipelineStateDesc _desc;

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;
    PipelineInputLayoutDX12 _inputLayout;

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
        _desc = new GraphicsPipelineStateDesc()
        {
            Flags = PipelineStateFlags.None,
            RasterizerState = _stateRasterizer.Desc,
            BlendState = _stateBlend.Description.Desc,
            DepthStencilState = _stateDepth.Description.Desc,
            SampleMask = _stateBlend.Description.BlendSampleMask,
            InputLayout = _inputLayout.Desc
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

    internal void Build()
    {
        if (IsBuilt)
            throw new Exception("Cannot build a pipeline state that has already been built.");

        _desc = new GraphicsPipelineStateDesc()
        {
            RasterizerState = _stateRasterizer.Desc,
            BlendState = _stateBlend.Description.Desc,
            DepthStencilState = _stateDepth.Description.Desc,
            Flags = PipelineStateFlags.None,
        };

        IsBuilt = true;
    }

    public bool Equals(PipelineStateDX12 other)
    {
        if(this == other)
            return true;

        if(_stateRasterizer != other._stateRasterizer
            || _stateBlend != other.BlendState
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
        NativeUtil.ReleasePtr(ref _state);
    }

    internal bool IsTemplate { get; }

    internal ID3D12PipelineState* NativePtr => _state;

    internal RasterizerStateDX12 RasterizerState
    {
        get => _stateRasterizer;
        set
        {
            if (IsBuilt)
                throw new Exception("Cannot change the rasterizer state of a pipeline state that has already been built.");

            _stateRasterizer = value;
        }
    }

    internal BlendStateDX12 BlendState
    {
        get => _stateBlend;
        set
        {
            if (IsBuilt)
                throw new Exception("Cannot change the blend state of a pipeline state that has already been built.");

            _stateBlend = value;
        }
    }

    internal DepthStateDX12 DepthState
    {
        get => _stateDepth;
        set
        {
            if (IsBuilt)
                throw new Exception("Cannot change the depth state of a pipeline state that has already been built.");

            _stateDepth = value;
        }
    }

    internal PipelineInputLayoutDX12 InputLayout
    {
        get => _inputLayout;
        set
        {
            if (IsBuilt)
                throw new Exception("Cannot change the input layout of a pipeline state that has already been built.");

            _inputLayout = value;
        }
    }

    public bool IsBuilt { get; private set; }

}
