using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    public abstract class MagickTextureWriter : TextureWriter
    {
        MagickFormat _writeFormat;

        protected MagickTextureWriter(MagickFormat writeFormat)
        {
            _writeFormat = writeFormat;
        }

        public override void WriteData(Stream stream, TextureData data, Logger log, string filename = null)
        {
            TextureData newData = data.Clone();
            newData.ToRGBA();

            TextureData.Slice slice;
            for (int i = 0; i < newData.Levels.Length; i++)
            {
                slice = newData.Levels[i];
                using (MagickImage image = new MagickImage(MagickColor.FromRgba(0,0,0,0), slice.Width, slice.Height))
                {
                    IPixelCollection p = image.GetPixels();
                    p.SetPixels(slice.Data);

                    image.Format = MagickFormat.Rgba;
                    image.Quality = 100;
                    image.Interlace = Interlace.NoInterlace;
                    image.Write(stream, _writeFormat);
                }
            }
        }
    }

    public class PNGWriter : MagickTextureWriter
    {
        public PNGWriter() : base(MagickFormat.Png) { }
    }

    public class JPEGWriter : MagickTextureWriter
    {
        public JPEGWriter() : base(MagickFormat.Jpeg) { }
    }

    public class BMPWriter : MagickTextureWriter
    {
        public BMPWriter() : base(MagickFormat.Bmp) { }
    }
}
