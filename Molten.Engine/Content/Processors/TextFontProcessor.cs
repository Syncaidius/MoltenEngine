using Molten.Graphics;

namespace Molten.Content
{
    public class TextFontProcessor : ContentProcessor<SpriteFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(TextFont), typeof(TextFontSource) };

        public override Type[] RequiredServices => null;

        protected override void OnRead(ContentContext context, SpriteFontParameters p)
        {
            TextFontSource tfs = context.Engine.Fonts.GetFont(context.Log, context.Filename);
            if (tfs != null)
            {
                if (context.ContentType == typeof(TextFont))
                {
                    TextFont tf = new TextFont(tfs, p.FontSize);
                    context.AddOutput(tf);
                }
                else if(context.ContentType == typeof(TextFontSource))
                {
                    context.AddOutput(tfs);
                }
            }
        }

        protected override void OnWrite(ContentContext context, SpriteFontParameters p)
        {
            throw new NotImplementedException();
        }
    }
}
