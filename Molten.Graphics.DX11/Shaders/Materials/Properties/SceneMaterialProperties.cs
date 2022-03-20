namespace Molten.Graphics
{
    internal class SceneMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue View { get; private set; }

        internal IShaderValue Projection { get; private set; }

        internal IShaderValue ViewProjection { get; private set; }

        internal IShaderValue InvViewProjection { get; private set; }

        internal IShaderValue MaxSurfaceUV { get; private set; }

        internal SceneMaterialProperties(Material material) : base(material)
        {
            View = MapValue(material, "view");
            Projection = MapValue(material, "projection");
            ViewProjection = MapValue(material, "viewProjection");
            InvViewProjection = MapValue(material, "invViewProjection");
            MaxSurfaceUV = MapValue(material, "maxSurfaceUV");
        }
    }
}
