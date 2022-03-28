namespace Molten.Graphics
{
    internal class GBufferTextureProperties : CommonShaderProperties
    {
        internal IShaderValue DiffuseTexture { get; set; }

        internal IShaderValue DiffuseTextureMS { get; set; }
        internal IShaderValue SampleCount { get; set; }

        internal IShaderValue NormalTexture { get; set; }

        internal IShaderValue EmissiveTexture { get; set; }

        internal GBufferTextureProperties(Material material)  : base(material)
        {
            DiffuseTexture = MapValue(material, "mapDiffuse");
            DiffuseTextureMS = MapValue(material, "mapDiffuseMS");
            SampleCount = MapValue(material, "sampleCount");
            NormalTexture = MapValue(material, "mapNormal");
            EmissiveTexture = MapValue(material, "mapEmissive");
        }
    }
}
