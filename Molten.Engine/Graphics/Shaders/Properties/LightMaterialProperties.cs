namespace Molten.Graphics
{
    public class LightMaterialProperties : CommonShaderProperties
    {
        public IShaderValue Data { get; set; }

        public IShaderValue MapDiffuse { get; set; }

        public IShaderValue MapNormal { get; set; }

        public IShaderValue MapDepth { get; set; }

        public IShaderValue InvViewProjection { get; set; }

        public IShaderValue CameraPosition { get; set; }

        public LightMaterialProperties(HlslShader shader) : base(shader)
        {
            Data = MapValue(shader, "LightData");
            MapDiffuse = MapValue(shader, "mapDiffuse");
            MapNormal = MapValue(shader, "mapNormal");
            MapDepth = MapValue(shader, "mapDepth");
            InvViewProjection = MapValue(shader, "invViewProjection");
            CameraPosition = MapValue(shader, "cameraPosition");
        }
    }
}
