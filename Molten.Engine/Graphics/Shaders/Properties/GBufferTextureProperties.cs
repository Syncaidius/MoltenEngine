namespace Molten.Graphics;

public class GBufferTextureProperties : CommonShaderProperties
{
    public ShaderVariable DiffuseTexture { get; set; }

    public ShaderVariable DiffuseTextureMS { get; set; }

    public ShaderVariable SampleCount { get; set; }

    public ShaderVariable NormalTexture { get; set; }

    public ShaderVariable EmissiveTexture { get; set; }

    public GBufferTextureProperties(Shader shader)  : base(shader)
    {
        DiffuseTexture = MapValue(shader, "mapDiffuse");
        DiffuseTextureMS = MapValue(shader, "mapDiffuseMS");
        SampleCount = MapValue(shader, "sampleCount");
        NormalTexture = MapValue(shader, "mapNormal");
        EmissiveTexture = MapValue(shader, "mapEmissive");
    }
}
