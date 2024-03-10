using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public unsafe class RasterizerStateVK : GpuObject<DeviceVK>, IEquatable<RasterizerStateVK>, IEquatable<PipelineRasterizationStateCreateInfo>
{
    PipelineRasterizationStateCreateInfo* _desc;

    public RasterizerStateVK(DeviceVK device, ref ShaderPassParameters parameters) :
        base(device)
    {
        _desc = EngineUtil.Alloc<PipelineRasterizationStateCreateInfo>();
        _desc[0] = new PipelineRasterizationStateCreateInfo()
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            PNext = null,
            PolygonMode = parameters.FillMode.ToApi(),
            CullMode = parameters.CullMode.ToApi(),
            DepthBiasClamp = parameters.DepthBiasClamp,
            DepthBiasSlopeFactor = parameters.SlopeScaledDepthBias,
            DepthClampEnable = parameters.DepthBiasEnabled,
            DepthBiasConstantFactor = parameters.DepthBias,
            FrontFace = parameters.IsFrontCounterClockwise ? FrontFace.CounterClockwise : FrontFace.Clockwise,
            RasterizerDiscardEnable = parameters.RasterizerDiscardEnabled,
            LineWidth = parameters.LineWidth,
            Flags = 0, // Reserved for use in future Vulkan versions.
        };
    }

    public override bool Equals(object obj) => obj switch
    {
        RasterizerStateVK val => Equals(*val._desc),
        PipelineRasterizationStateCreateInfo val => Equals(val),
        _ => false
    };

    public bool Equals(RasterizerStateVK other)
    {
        return Equals(*other._desc);
    }

    public bool Equals(PipelineRasterizationStateCreateInfo other)
    {
        return _desc->PolygonMode == other.PolygonMode
            && _desc->CullMode == other.CullMode
            && _desc->DepthBiasClamp == other.DepthBiasClamp
            && _desc->DepthBiasSlopeFactor == other.DepthBiasSlopeFactor
            && _desc->DepthClampEnable.Value == other.DepthClampEnable.Value
            && _desc->DepthBiasConstantFactor == other.DepthBiasConstantFactor
            && _desc->FrontFace == other.FrontFace
            && _desc->RasterizerDiscardEnable.Value == other.RasterizerDiscardEnable.Value
            && _desc->LineWidth == other.LineWidth
            && _desc->Flags == other.Flags;
    }

    protected override void OnGraphicsRelease() { }

    internal PipelineRasterizationStateCreateInfo* Desc => _desc;
}
