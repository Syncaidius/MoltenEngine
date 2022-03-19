using Molten.Font;
using Molten.Graphics;
using System;
using System.IO;

namespace Molten.Content
{
    public class SpriteFontProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(SpriteFont) };

        public override Type[] RequiredServices => null;

        protected override void OnInitialize()
        {
            AddParameter("size", 18);
        }

        public override void OnRead(ContentContext context)
        {
            int size = context.Parameters.Get<int>("size");

            SpriteFont sf = context.Engine.Fonts.GetFont(context.Log, context.Filename, size);
            if (sf != null)
                context.AddOutput(sf);
        }

        public override void OnWrite(ContentContext context)
        {
            throw new NotImplementedException();
        }
    }
}
