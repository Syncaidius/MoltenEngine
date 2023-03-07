namespace Molten.Graphics
{
    public class SceneMaterialProperties : CommonShaderProperties
    {
        public IShaderValue View { get; private set; }

        public IShaderValue Projection { get; private set; }

        public IShaderValue ViewProjection { get; private set; }

        public IShaderValue InvViewProjection { get; private set; }

        public IShaderValue MaxSurfaceUV { get; private set; }

        public SceneMaterialProperties(HlslShader shader) : base(shader)
        {
            View = MapValue(shader, "view");
            Projection = MapValue(shader, "projection");
            ViewProjection = MapValue(shader, "viewProjection");
            InvViewProjection = MapValue(shader, "invViewProjection");
            MaxSurfaceUV = MapValue(shader, "maxSurfaceUV");
        }
    }
}
