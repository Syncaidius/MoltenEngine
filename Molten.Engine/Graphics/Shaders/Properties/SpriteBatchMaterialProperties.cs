namespace Molten.Graphics
{
    public class SpriteBatchMaterialProperties : CommonShaderProperties
    {
        public IShaderValue TextureSize { get; set; }

        public SpriteBatchMaterialProperties(HlslShader shader) : base(shader)
        {
            TextureSize = MapValue(shader, "textureSize");
        }
    }
}
