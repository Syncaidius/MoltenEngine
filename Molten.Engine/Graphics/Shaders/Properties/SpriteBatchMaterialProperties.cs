namespace Molten.Graphics;

public class SpriteBatchMaterialProperties : CommonShaderProperties
{
    public ShaderVariable TextureSize { get; set; }

    public SpriteBatchMaterialProperties(Shader shader) : base(shader)
    {
        TextureSize = MapValue(shader, "textureSize");
    }
}
