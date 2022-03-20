namespace Molten.Graphics
{
    internal class LightMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue Data { get; set; }

        internal IShaderValue MapDiffuse { get; set; }

        internal IShaderValue MapNormal { get; set; }

        internal IShaderValue MapDepth { get; set; }

        internal IShaderValue InvViewProjection { get; set; }

        internal IShaderValue CameraPosition { get; set; }

        internal LightMaterialProperties(Material material) : base(material)
        {
            Data = MapValue(material, "LightData");
            MapDiffuse = MapValue(material, "mapDiffuse");
            MapNormal = MapValue(material, "mapNormal");
            MapDepth = MapValue(material, "mapDepth");
            InvViewProjection = MapValue(material, "invViewProjection");
            CameraPosition = MapValue(material, "cameraPosition");
        }
    }
}
