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

        public override void OnRead(ContentContext context)
        {
            int size = 18;
            string strSize = "";
            if (context.Metadata.TryGetValue("size", out strSize))
                int.TryParse(strSize, out size);
            else
                context.Log.Log($"No font size specified. Using default of {size}");

            FontFile font = null;
            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, context.Log, context.Filename))
                    font = reader.ReadFont(true);
            }

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
