namespace Molten
{
    public class TextFontParameters : ContentParameters
    {
        public float FontSize { get; set; } = 16;

        public override object Clone()
        {
            return new TextFontParameters()
            {
                FontSize = FontSize,
                PartCount = PartCount
            };
        }
    }
}
