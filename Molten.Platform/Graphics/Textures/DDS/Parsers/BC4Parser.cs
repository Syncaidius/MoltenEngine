using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC4Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC4_UNorm };

        protected unsafe override void DecodeBlock(BinaryReader reader, BCDimensions dimensions, int width, int height, byte[] output)
        {
            byte red0 = reader.ReadByte();
            byte red1 = reader.ReadByte();

            // Read each of the sixteen 3-bit pixel indices, totaling 48 bits (6 bytes)
            ulong redMask = reader.ReadByte(); 
            redMask += (ulong)reader.ReadByte() << 8;
            redMask += (ulong)reader.ReadByte() << 16;
            redMask += (ulong)reader.ReadByte() << 24;
            redMask += (ulong)reader.ReadByte() << 32;
            redMask += (ulong)reader.ReadByte() << 40;

            // Decompress pixel data from block
            for (int pY = 0; pY < DDSHelper.BLOCK_DIMENSIONS; pY++)
            {
                for (int pX = 0; pX < DDSHelper.BLOCK_DIMENSIONS; pX++)
                {
                    ulong redIndex = ((redMask >> 3 * (4 * pY + pX)) & 0x07);
                    float c = 0;

                    // Decode alpha
                    if (redIndex == 0)
                        c = red0;
                    else if (redIndex == 1)
                        c = red1;
                    else if (red0 > red1)
                        c = (redIndex * red0 + redIndex * red1) / 7.0f;
                    else if (redIndex == 6)
                        c = 0;
                    else if (redIndex == 7)
                        c = 255;
                    else
                        c = (redIndex * red0 + redIndex * red1) / 5.0f;

                    int px = (dimensions.X << 2) + pX;
                    int py = (dimensions.Y << 2) + pY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        byte cb = (byte)c;

                        output[offset] = cb;
                        output[offset + 1] = cb;
                        output[offset + 2] = cb;
                        output[offset + 3] = 255;
                    }
                }
            }
        }

        protected override void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int bytesPerPixel = 4;
            Encode8BitSingleChannelBlock(writer, level, ref dimensions, bytesPerPixel, 0);
        }
    }
}
