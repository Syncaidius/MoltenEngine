using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC2Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC2_Typeless, GraphicsFormat.BC2_UNorm, GraphicsFormat.BC2_UNorm_SRgb };

        protected override void DecompressBlock(BinaryReader imageReader, int x, int y, int width, int height, byte[] output)
        {
            byte a0 = imageReader.ReadByte();
            byte a1 = imageReader.ReadByte();
            byte a2 = imageReader.ReadByte();
            byte a3 = imageReader.ReadByte();
            byte a4 = imageReader.ReadByte();
            byte a5 = imageReader.ReadByte();
            byte a6 = imageReader.ReadByte();
            byte a7 = imageReader.ReadByte();

            DDSColorTable table;
            DecompressColorTableDXT1(imageReader, out table);

            int alphaIndex = 0;

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    Color c = new Color();

                    uint index = (table.data >> 2 * (4 * blockY + blockX)) & 0x03;

                    c = table.color[index];

                    switch (alphaIndex)
                    {
                        case 0:
                            c.A = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4));
                            break;
                        case 1:
                            c.A = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
                            break;
                        case 2:
                            c.A = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4));
                            break;
                        case 3:
                            c.A = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
                            break;
                        case 4:
                            c.A = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4));
                            break;
                        case 5:
                            c.A = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
                            break;
                        case 6:
                            c.A = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4));
                            break;
                        case 7:
                            c.A = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
                            break;
                        case 8:
                            c.A = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4));
                            break;
                        case 9:
                            c.A = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
                            break;
                        case 10:
                            c.A = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4));
                            break;
                        case 11:
                            c.A = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
                            break;
                        case 12:
                            c.A = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4));
                            break;
                        case 13:
                            c.A = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
                            break;
                        case 14:
                            c.A = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4));
                            break;
                        case 15:
                            c.A = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
                            break;
                    }
                    alphaIndex++;

                    // Store decompressed color data.
                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        output[offset] = c.R;
                        output[offset + 1] = c.G;
                        output[offset + 2] = c.B;
                        output[offset + 3] = c.A;
                    }
                }
            }
        }

        protected override void CompressBlock(BinaryWriter writer, int bX, int bY, TextureData.Slice level)
        {
            // Get the pixel position of the block. Each block is 4x4 pixels.
            int bPixelX = bX * 4;
            int bPixelY = bY * 4;
            int colorByteSize = 4;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x += 2) //Increment by 2 pixels each iteration.
                {
                    int pX = bPixelX + x;
                    int pY = bPixelY + y;
                    int b = GetPixelByte(pX, pY, level.Width, colorByteSize) + 3; // add 3 bytes to access alpha

                    byte a1 = level.Data[b];
                    byte a2 = level.Data[b + 4];
                    byte result = (byte)(((a2 >> 4) << 4) | (a1 >> 4));
                    writer.Write(result);
                }
            }

            // Write color data
            CompressDXT1Block(writer, level, bPixelX, bPixelY, colorByteSize, false, 0);
        }
    }
}
