using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>, IEquatable<PipelineStateDX12>
{
    ID3D12PipelineState* _handle;
    GraphicsPipelineStateDesc _desc;

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;
    PipelineInputLayoutDX12 _inputLayout;
    RootSignatureDX12 _rootSignature;

    public PipelineStateDX12(PipelineStateDX12 template, bool isTemplate) : base(template.Device)
    {
        _desc = template._desc;
        IsTemplate = isTemplate;
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

        _desc = new GraphicsPipelineStateDesc()
        {
            Flags = PipelineStateFlags.None,
            PRootSignature = _rootSignature,
            RasterizerState = _stateRasterizer.Desc,
            BlendState = _stateBlend.Description.Desc,
            SampleMask = _stateBlend.Description.BlendSampleMask,
            DepthStencilState = _stateDepth.Description.Desc,
            VS = pass.GetBytecode(ShaderType.Vertex),
            GS = pass.GetBytecode(ShaderType.Geometry),
            DS = pass.GetBytecode(ShaderType.Domain),
            HS = pass.GetBytecode(ShaderType.Hull),
            PS = pass.GetBytecode(ShaderType.Pixel),
        };

        // Find out how many render targets to expect.
        ShaderComposition ps = pass[ShaderType.Pixel];
        if(ps != null)
            _desc.NumRenderTargets = (uint)ps.OutputLayout.Metadata.Length;

        IsTemplate = isTemplate;
    }

    internal void SetRenderTargetFormats(IRenderSurface[] surfaces)
    {
        if (IsBuilt)
            throw new Exception("Cannot change the render target formats of a pipeline state that has already been built.");

        if(_desc.NumRenderTargets != surfaces.Length)
            throw new Exception($"Pipeline state is expecting {_desc.NumRenderTargets} surfaces, but received {surfaces.Length}.");

        for(int i = 0; i < _desc.NumRenderTargets; i++)
            _desc.RTVFormats[i] = surfaces[i].ResourceFormat.ToApi();
    }

    /// <summary>
    /// Builds the current <see cref="PipelineStateDX12"/>. The pipeline state is only compiled if it does not match any other cached pipeline state.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal PipelineStateDX12 Build()
    {
        if (IsBuilt)
            throw new Exception("Cannot build a pipeline state that has already been built.");

        if(IsTemplate)
            throw new Exception("Tempalte pipeline states cannot be built.");

        if (_desc.NumRenderTargets > 0)
        {
            for (int i = 0; i < _desc.NumRenderTargets; i++)
            {
                if (_desc.RTVFormats[i] == Format.FormatUnknown)
                    throw new Exception($"Pipeline state is expecting {_desc.NumRenderTargets} render surfaces, but slot {i} formats is not defined.");
            }
        }

        _desc.InputLayout = _inputLayout.Desc;

        // Check cache for a matching PSO.
        PipelineStateDX12 result = this;
        Device.Cache.Add(result);

        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        HResult hr = Device.Ptr->CreateGraphicsPipelineState(_desc, &guid, &ptr);
        if(!Device.Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return null;

        _handle = (ID3D12PipelineState*)ptr;    

        IsBuilt = true;

        return result;
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

    public bool IsBuilt { get; private set; }
}
