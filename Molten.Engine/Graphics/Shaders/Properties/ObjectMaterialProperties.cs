namespace Molten.Graphics;

public class ObjectMaterialProperties : CommonShaderProperties
{
    public ShaderVariable World { get; set; }

    public ShaderVariable Wvp { get; set; }

    public ShaderVariable EmissivePower { get; set; }

    public ObjectMaterialProperties(HlslShader shader) : base(shader)
    {
        World = MapValue(shader, "world");
        Wvp = MapValue(shader, "wvp");
        EmissivePower = MapValue(shader, "emissivePower");
    }
}
