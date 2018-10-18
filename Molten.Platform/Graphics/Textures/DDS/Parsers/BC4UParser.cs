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

        protected unsafe override void DecompressBlock(BinaryReader reader, int blockX, int blockY, int width, int height, byte[] output)
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
            for (int pY = 0; pY < 4; pY++)
            {
                for (int pX = 0; pX < 4; pX++)
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

                    int px = (blockX << 2) + pX;
                    int py = (blockY << 2) + pY;
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

        protected override void CompressBlock(BinaryWriter writer, int bX, int bY, TextureData.Slice level)
        {
            int bPixelX = bX * 4;
            int bPixelY = bY * 4;
            int colorByteSize = 1;

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

            writer.Write(red[0]);
            writer.Write(red[1]);

            ulong redMask = 0;
            int rID = 0;

            // Build 6-byte alpha mask by generating 16x 3-bit pixel indices.
            // Note: 16x 3-bit pixel indices = 48 bits (6 bytes).
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    float closest = float.MaxValue;
                    int closestID = 7;

                    int pX = bPixelX + x;
                    int pY = bPixelY + y;

                    int destByte = GetByteRGBA(pX, pY, level.Width, colorByteSize);

                    // Test distance of each color table entry
                    for (int i = 0; i < red.Length; i++)
                    {
                        float dist = Math.Abs(red[i] - level.Data[destByte]);
                        if (dist < closest)
                        {
                            closest = dist;
                            closestID = i;
                        }
                    }

                    // Write the ID of the closest alpha value
                    redMask |= (ulong)closestID << (3 * rID);
                    rID++;
                }
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

                        int destByte = GetByteRGBA(pX, pY, level.Width, colorByteSize);
                        if (level.Data[destByte] > result)
                            result = level.Data[destByte];
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

                        int b = GetByteRGBA(pX, pY, level.Width, colorByteSize);
                        if (level.Data[b] < result)
                            result = level.Data[b];
                    }
                }

                return result;
            }
        }
    }
}
