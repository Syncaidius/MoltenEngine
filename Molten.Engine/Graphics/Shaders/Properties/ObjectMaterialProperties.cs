namespace Molten.Graphics
{
    public class ObjectMaterialProperties : CommonShaderProperties
    {
        public IShaderValue World { get; set; }

        public IShaderValue Wvp { get; set; }

        public IShaderValue EmissivePower { get; set; }

        public ObjectMaterialProperties(HlslShader shader) : base(shader)
        {
            World = MapValue(shader, "world");
            Wvp = MapValue(shader, "wvp");
            EmissivePower = MapValue(shader, "emissivePower");
        }
    }
}
