namespace Molten.Graphics
{
    internal class GBufferTextureProperties : CommonShaderProperties
    {
        internal IShaderValue DiffuseTexture { get; set; }

        internal IShaderValue DiffuseTextureMS { get; set; }

        internal IShaderValue NormalTexture { get; set; }

        internal IShaderValue EmissiveTexture { get; set; }

        internal GBufferTextureProperties(Material material)  : base(material)
        {
            DiffuseTexture = MapValue(material, "mapDiffuse");
            DiffuseTextureMS = MapValue(material, "mapDiffuseMS");
            NormalTexture = MapValue(material, "mapNormal");
            EmissiveTexture = MapValue(material, "mapEmissive");
        }
    }
}
