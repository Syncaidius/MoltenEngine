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

        public override void OnRead(Engine engine, Logger log, Type contentType, Stream stream, Dictionary<string,string> metadata, FileInfo file, ContentResult output)
        {
            int size = 18;
            string strSize = "";
            if (metadata.TryGetValue("size", out strSize))
                int.TryParse(strSize, out size);

            FontFile font = null;
            using (FontReader reader = new FontReader(stream, log, file.FullName))
                font = reader.ReadFont(true);

            if (!font.HasFlag(FontFlags.Invalid))
                output.AddResult(new SpriteFont(engine.Renderer, font, size));
        }

        public override void OnWrite(Engine engine, Logger log, Type t, Stream stream, FileInfo file)
        {
            throw new NotImplementedException();
        }
    }
}
