namespace Molten
{
    public class ShaderParameters : ContentParameters
    {
        public string MaterialName = "";

        public override object Clone()
        {
            return new ShaderParameters()
            {
                MaterialName = MaterialName,
                PartCount = PartCount
            };
        }
    }
}
