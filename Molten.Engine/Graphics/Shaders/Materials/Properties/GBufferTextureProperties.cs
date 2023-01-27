namespace Molten.Graphics
{
    public class GBufferTextureProperties : CommonShaderProperties
    {
        public IShaderValue DiffuseTexture { get; set; }

        public IShaderValue DiffuseTextureMS { get; set; }

        public IShaderValue SampleCount { get; set; }

        public IShaderValue NormalTexture { get; set; }

        public IShaderValue EmissiveTexture { get; set; }

        public GBufferTextureProperties(HlslShader shader)  : base(shader)
        {
            DiffuseTexture = MapValue(shader, "mapDiffuse");
            DiffuseTextureMS = MapValue(shader, "mapDiffuseMS");
            SampleCount = MapValue(shader, "sampleCount");
            NormalTexture = MapValue(shader, "mapNormal");
            EmissiveTexture = MapValue(shader, "mapEmissive");
        }
    }
}
