namespace Molten.Graphics;
public  class MeshShaderCapabilities : ShaderStageCapabilities
{
    public bool PipelineStatsSupported { get; set; }

    public bool PerPrimitiveShadingRate { get; set; }
}
