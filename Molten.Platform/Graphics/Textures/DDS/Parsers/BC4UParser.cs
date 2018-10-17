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

        protected override void DecompressBlock(BinaryReader reader, int x, int y, int width, int height, byte[] output)
        {
            //===========ALPHA BLOCK==========================
            byte red0 = reader.ReadByte();
            byte red1 = reader.ReadByte();
            ulong redMask = reader.ReadByte(); // Contains sixteen 3-bit pixel indices, totalling 48 bits (6 bytes)

            redMask += (ulong)reader.ReadByte() << 8;
            redMask += (ulong)reader.ReadByte() << 16;
            redMask += (ulong)reader.ReadByte() << 24;
            redMask += (ulong)reader.ReadByte() << 32;
            redMask += (ulong)reader.ReadByte() << 40;

            // Decompress pixel data from block
            for (int pY = 0; pY < 4; pY++)
            {
                for (int pX = 0; pX < 4; pX++)
                {
                    uint redIndex = (uint)((redMask >> 3 * (4 * pY + pX)) & 0x07);
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

                    int px = (x << 2) + pX;
                    int py = (y << 2) + pY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        byte cb = (byte)(255f * c);

                        output[offset] = cb;
                        output[offset + 1] = cb;
                        output[offset + 2] = cb;
                        output[offset + 3] = 255;
                    }
                }
            }
        }

        protected override void CompressBlock(BinaryWriter writer, int bX, int bY, TextureData.Slice level)
        {
            int bPixelX = bX * 4;
            int bPixelY = bY * 4;
            int colorByteSize = 4;

            float[] red = new float[8];
            red[0] = GetRed(false, level, bPixelX, bPixelY, colorByteSize);
            red[1] = GetRed(true, level, bPixelX, bPixelY, colorByteSize);

            // Interpolate alpha 2 - 7 values
            if (red[0] > red[1])
            {
                // 6 interpolated color values
                red[2] = (6 * red[0] + 1 * red[1]) / 7.0f; // bit code 010
                red[3] = (5 * red[0] + 2 * red[1]) / 7.0f; // bit code 011
                red[4] = (4 * red[0] + 3 * red[1]) / 7.0f; // bit code 100
                red[5] = (3 * red[0] + 4 * red[1]) / 7.0f; // bit code 101
                red[6] = (2 * red[0] + 5 * red[1]) / 7.0f; // bit code 110
                red[7] = (1 * red[0] + 6 * red[1]) / 7.0f; // bit code 111
            }
            else
            {
                // 4 interpolated color values
                red[2] = (4 * red[0] + 1 * red[1]) / 5.0f; // bit code 010
                red[3] = (3 * red[0] + 2 * red[1]) / 5.0f; // bit code 011
                red[4] = (2 * red[0] + 3 * red[1]) / 5.0f; // bit code 100
                red[5] = (1 * red[0] + 4 * red[1]) / 5.0f; // bit code 101
                red[6] = 0.0f;                     // bit code 110
                red[7] = 1.0f;                     // bit code 111
            }
        }

        private byte GetRed(bool getHighest, TextureData.Slice level, int blockPixelX, int blockPixelY, int colorByteSize)
        {
            int pitch = level.Width * 4;

            if (getHighest)
            {
                byte result = 0;

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        int pX = blockPixelX + x;
                        int pY = blockPixelY + y;

                        int b = GetPixelByte(pX, pY, level.Width, colorByteSize) + 3; // Add 3 bytes to reach the alpha
                        if (level.Data[b] > result)
                            result = level.Data[b];
                    }
                }

                return result;
            }
            else
            {
                byte result = 255;

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        int pX = blockPixelX + x;
                        int pY = blockPixelY + y;

                        int b = GetPixelByte(pX, pY, level.Width, colorByteSize) + 3; // Add 3 bytes to reach the alpha
                        if (level.Data[b] < result)
                            result = level.Data[b];
                    }
                }

                return result;
            }
        }
    }
}
