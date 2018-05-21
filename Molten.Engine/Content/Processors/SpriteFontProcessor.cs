using Molten.Font;
using Molten.Graphics;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Content
{
    public class SpriteFontProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; protected set; } = new Type[] { typeof(SpriteFont)};

        public override void OnRead(ContentContext context)
        {
            int size = 18;
            string strSize = "";
            if (context.Metadata.TryGetValue("size", out strSize))
                int.TryParse(strSize, out size);
            else
                context.Log.WriteLine($"No font size specified. Using default of {size}");

            FontFile font = null;
            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, context.Log, context.Filename))
                    font = reader.ReadFont(true);
            }

            if (!font.HasFlag(FontFlags.Invalid))
                context.AddOutput(new SpriteFont(context.Engine.Renderer, font, size));
        }

        public override void OnWrite(ContentContext context)
        {
            throw new NotImplementedException();
        }
    }
}
