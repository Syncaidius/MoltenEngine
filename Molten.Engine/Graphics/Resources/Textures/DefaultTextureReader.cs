using ImageMagick;

namespace Molten.Graphics.Textures;

public class DefaultTextureReader : TextureReader
{
    Logger _log;
    string _filename;

    public unsafe override TextureData Read(BinaryReader reader, Logger log, string filename = null)
    {
        _log = log;
        _filename = null;

        MagickImage image = new MagickImage(reader.BaseStream);
        image.Warning += Image_Warning;
        IPixelCollection<byte> pixels = image.GetPixels();
        byte[] bPixels = pixels.ToByteArray(PixelMapping.RGBA);
        TextureSlice slice = new TextureSlice((uint)image.Width, (uint)image.Height, 1, bPixels)
        {
            Pitch = (uint)image.Width * 4U // We're using 4 bytes per pixel (RGBA)
        };

        TextureData data = new TextureData(1, slice)
        {
            IsCompressed = false,
            HighestMipMap = 0,
            MultiSampleLevel = AntiAliasLevel.None,
            Flags = GpuResourceFlags.GpuWrite,
            Format = GpuResourceFormat.R8G8B8A8_UNorm,
        };

        image.Warning -= Image_Warning;
        image.Dispose();
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
