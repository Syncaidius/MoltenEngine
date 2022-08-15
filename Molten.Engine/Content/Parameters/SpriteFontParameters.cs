namespace Molten
{
    public class SpriteFontParameters : ContentParameters
    {
        public float FontSize { get; set; } = 16;

        public override object Clone()
        {
            return new SpriteFontParameters()
            {
                FontSize = FontSize,
                PartCount = PartCount
            };
        }
    }
}
