using Molten.Graphics;

namespace Molten.Content
{
    public class SpriteFontProcessor : ContentProcessor<SpriteFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(SpriteFont) };

        public override Type[] RequiredServices => null;

        protected override void OnRead(ContentContext context, SpriteFontParameters p)
        {
            SpriteFont sf = context.Engine.Fonts.GetFont(context.Log, context.Filename);
            if (sf != null)
                context.AddOutput(sf);
        }

        protected override void OnWrite(ContentContext context, SpriteFontParameters p)
        {
            throw new NotImplementedException();
        }
    }
}
