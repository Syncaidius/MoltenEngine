using ImageMagick;

namespace Molten.Graphics.Textures
{
    public class DefaultTextureReader : TextureReader
    {
        Logger _log;
        string _filename;

        public unsafe override TextureData Read(BinaryReader reader, Logger log, string filename = null)
        {
            _log = log;
            _filename = null;

            TextureData data = new TextureData()
            {
                IsCompressed = false,
                HighestMipMap = 0,
                MipMapLevels = 1,
                MultiSampleLevel = AntiAliasLevel.None,
                Flags = TextureFlags.None,
                Format = GraphicsFormat.R8G8B8A8_UNorm,
            };

            using (MagickImage image = new MagickImage(reader.BaseStream))
            {
                image.Warning += Image_Warning;
                data.Width = (uint)image.Width;
                data.Height = (uint)image.Height;
                IPixelCollection<byte> pixels = image.GetPixels();
                byte[] bPixels = pixels.ToByteArray(PixelMapping.RGBA);
                TextureData.Slice slice = new TextureData.Slice(bPixels, (uint)bPixels.Length)
                {
                    Width = data.Width,
                    Height = data.Height,
                    Pitch = data.Width * 4 // We're using 4 bytes per pixel (RGBA)
                };

                data.Levels = new TextureData.Slice[] { slice };
                image.Warning -= Image_Warning;
            }

            _log = null;
            return data;
        }

        private void Image_Warning(object sender, WarningEventArgs e)
        {
            _log.Warning(e.Message, _filename);
            if (e.Exception != null)
                _log.Error(e.Exception, true);
        }
    }
}
