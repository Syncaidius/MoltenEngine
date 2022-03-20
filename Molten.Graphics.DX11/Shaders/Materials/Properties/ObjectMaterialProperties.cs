namespace Molten.Graphics
{
    internal class ObjectMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue World { get; set; }

        internal IShaderValue Wvp { get; set; }

        internal IShaderValue EmissivePower { get; set; }

        internal ObjectMaterialProperties(Material material) : base(material)
        {
            World = MapValue(material, "world");
            Wvp = MapValue(material, "wvp");
            EmissivePower = MapValue(material, "emissivePower");
        }
    }
}
