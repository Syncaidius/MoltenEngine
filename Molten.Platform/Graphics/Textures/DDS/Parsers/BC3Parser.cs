using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC3Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC3_Typeless, GraphicsFormat.BC3_UNorm, GraphicsFormat.BC3_UNorm_SRgb };

        protected override void DecodeBlock(BinaryReader reader, BCDimensions dimensions, int width, int height, byte[] output)
        {
            //===========ALPHA BLOCK==========================
            byte alpha0 = reader.ReadByte();
            byte alpha1 = reader.ReadByte();
            ulong alphaMask = reader.ReadByte();

            alphaMask += (ulong)reader.ReadByte() << 8;
            alphaMask += (ulong)reader.ReadByte() << 16;
            alphaMask += (ulong)reader.ReadByte() << 24;
            alphaMask += (ulong)reader.ReadByte() << 32;
            alphaMask += (ulong)reader.ReadByte() << 40;

            //============COLOR BLOCK=========================
            BCColorTable table;
            DecodeColorTableBC1(reader, out table);

            // Decompress pixel data from block
            for (int pby = 0; pby < DDSHelper.BLOCK_DIMENSIONS; pby++)
            {
                int py = (dimensions.Y << 2) + pby;

                for (int pbx = 0; pbx < DDSHelper.BLOCK_DIMENSIONS; pbx++)
                {
                    uint index = (table.data >> 2 * (4 * pby + pbx)) & 0x03;
                    uint alphaIndex = (uint)((alphaMask >> 3 * (4 * pby + pbx)) & 0x07);
                    Color c = table.color[index];

                    // Decode alpha
                    if (alphaIndex == 0)
                        c.A = alpha0;
                    else if (alphaIndex == 1)
                        c.A = alpha1;
                    else if (alpha0 > alpha1)
                        c.A = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7f);
                    else if (alphaIndex == 6)
                        c.A = 0;
                    else if (alphaIndex == 7)
                        c.A = 255;
                    else
                        c.A = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5f);

                    int px = (dimensions.X << 2) + pbx;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        output[offset] = c.R;
                        output[offset + 1] = c.G;
                        output[offset + 2] = c.B;
                        output[offset + 3] = DecodeSingleChannelColor(alphaMask, pbx, pby, alpha0, alpha1);
                    }
                }
            }
        }

        protected override void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int bytesPerPixel = 4;
            Encode8BitSingleChannelBlock(writer, level, ref dimensions, bytesPerPixel, 3);
            EncodeBC1ColorBlock(writer, level, dimensions.PixelX, dimensions.PixelY, bytesPerPixel, false, 0, dimensions);
        }
    }
}
