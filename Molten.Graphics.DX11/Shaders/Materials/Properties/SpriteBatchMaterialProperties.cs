namespace Molten.Graphics
{
    internal class SpriteBatchMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue TextureSize { get; set; }

        internal SpriteBatchMaterialProperties(Material material) : base(material)
        {
            TextureSize = MapValue(material, "textureSize");
        }
    }
}
