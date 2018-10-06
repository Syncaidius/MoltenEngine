using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    public class DDSWriter : TextureWriter
    {
        DDSFormat _format;

        public DDSWriter(DDSFormat format)
        {
            _format = format;
        }

        public override void WriteData(Stream stream, TextureData data, Logger log)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Use DXT5 since this covers most types of images (transparent and opaque).
                if (data.IsCompressed == false)
                    data.Compress(_format);

                // Create new DDS header
                DDSHeader header = new DDSHeader()
                {
                    Size = 124,
                    Flags = DDSFlags.Capabilities | DDSFlags.Width | DDSFlags.Height | DDSFlags.MipMapCount | DDSFlags.PixelFormat | DDSFlags.LinearSize,
                    Height = (uint)data.Height,
                    Width = (uint)data.Width,
                    PitchOrLinearSize = (uint)data.Levels[0].TotalBytes,
                    Depth = 0,
                    MipMapCount = (uint)data.MipMapLevels,
                    Reserved = new uint[11],
                    PixelFormat = GetPixelFormat(data),
                    Caps = DDSCapabilities.Texture | DDSCapabilities.Complex | DDSCapabilities.MipMap,
                    Caps2 = DDSAdditionalCapabilities.None,
                    Caps3 = 0,
                    Caps4 = 0,
                    Reserved2 = 0,
                };

                WriteMagicWord(writer);
                WriteHeader(writer, ref header);

                // Write each mip map level
                for (int i = 0; i < data.MipMapLevels; i++)
                    writer.Write(data.Levels[i].Data);
            }
        }

        private DDSPixelFormat GetPixelFormat(TextureData data)
        {
            DDSPixelFormat result = new DDSPixelFormat();
            result.Size = 32;
            result.Flags = DDSPixelFormatFlags.FourCC;

            switch (data.Format)
            {
                case GraphicsFormat.BC1_UNorm:
                    result.FourCC = "DXT1";
                    result.RGBBitCount = 32;
                    break;

                case GraphicsFormat.BC2_UNorm:
                    result.FourCC = "DXT3";
                    result.RGBBitCount = 32;
                    break;

                default:
                case GraphicsFormat.BC3_UNorm:
                    result.FourCC = "DXT5";
                    result.RGBBitCount = 32;
                    break;

                /*case GraphicsFormat.BC4_UNorm:
                    result.FourCC = "DXT4";
                    result.RGBBitCount = 32;
                    break;

                default:
                case GraphicsFormat.BC5_SNorm:
                    result.FourCC = "DXT5";
                    result.RGBBitCount = 32;
                    break;*/
            }

            return result;
        }

        private uint GetFourCCValue(string cc)
        {
            // Create CC hash.
            uint result = (uint)(cc[3] << 24);
            result |= (uint)(cc[2] << 16);
            result |= (uint)(cc[1] << 8);
            result |= (uint)cc[0];

            return result;
        }

        private void WriteMagicWord(BinaryWriter writer)
        {
            char a = 'D';
            char b = 'D';
            char c = 'S';

            uint result = (uint)32 << 24;
            result |= (uint)c << 16;
            result |= (uint)b << 8;
            result |= (uint)a;

            writer.Write(result);
        }

        private void WriteHeader(BinaryWriter writer, ref DDSHeader header)
        {
            writer.Write(header.Size);
            writer.Write((uint)header.Flags);
            writer.Write(header.Height);
            writer.Write(header.Width);
            writer.Write(header.PitchOrLinearSize);
            writer.Write(header.Depth);
            writer.Write(header.MipMapCount);

            for(int i = 0; i < header.Reserved.Length; i++)
                writer.Write(header.Reserved[i]);

            writer.Write(header.PixelFormat.Size);
            writer.Write((uint)header.PixelFormat.Flags);

            string f = header.PixelFormat.FourCC;
            uint fourCC = GetFourCCValue(header.PixelFormat.FourCC);
            writer.Write(fourCC);

            writer.Write(header.PixelFormat.RGBBitCount);
            writer.Write(header.PixelFormat.RBitMask);
            writer.Write(header.PixelFormat.GBitMask);
            writer.Write(header.PixelFormat.BBitMask);
            writer.Write(header.PixelFormat.ABitMask);

            writer.Write((uint)header.Caps);
            writer.Write((uint)header.Caps2);
            writer.Write(header.Caps3);
            writer.Write(header.Caps4);
            writer.Write(header.Reserved2);
        }

        protected override void OnDispose()
        {
            
        }
    }
}
