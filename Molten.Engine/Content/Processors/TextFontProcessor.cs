using Molten.Graphics;

namespace Molten.Content
{
    public class TextFontProcessor : ContentProcessor<SpriteFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(TextFont) };

        public override Type[] RequiredServices => null;

        protected override void OnRead(ContentContext context, SpriteFontParameters p)
        {
            TextFontSource tfs = context.Engine.Fonts.GetFont(context.Log, context.Filename);
            if (tfs != null)
            {
                TextFont tf = new TextFont(tfs, p.FontSize);
                context.AddOutput(tf);
            }
        }

        protected override void OnWrite(ContentContext context, SpriteFontParameters p)
        {
            throw new NotImplementedException();
        }
    }
}
