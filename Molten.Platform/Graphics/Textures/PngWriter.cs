using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    public class PNGWriter : TextureWriter
    {
        public override void WriteData(Stream stream, TextureData data, Logger log)
        {
            for(int i = 0; i < data.Levels.Length; i++)
            {
                using (MagickImage image = new MagickImage(data.Levels[i].Data))
                {
                    image.Write(stream, MagickFormat.Png);
                }
            }
        }
    }
}
