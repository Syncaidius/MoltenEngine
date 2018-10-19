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

        protected override void DecompressBlock(BinaryReader reader, BCDimensions dimensions, int width, int height, byte[] output)
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
            DDSColorTable table;
            DecompressColorTableBC1(reader, out table);

            // Decompress pixel data from block
            for (int pY = 0; pY < 4; pY++)
            {
                int py = (dimensions.Y << 2) + pY;
                for (int pX = 0; pX < 4; pX++)
                {
                    uint index = (table.data >> 2 * (4 * pY + pX)) & 0x03;
                    uint alphaIndex = (uint)((alphaMask >> 3 * (4 * pY + pX)) & 0x07);
                    Color c = table.color[index];

                    // Decode alpha
                    if (alphaIndex == 0)
                        c.A = alpha0;
                    else if (alphaIndex == 1)
                        c.A = alpha1;
                    else if (alpha0 > alpha1)
                        c.A = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    else if (alphaIndex == 6)
                        c.A = 0;
                    else if (alphaIndex == 7)
                        c.A = 255;
                    else
                        c.A = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);

                    int px = (dimensions.X << 2) + pX;
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

        protected override void CompressBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int bPixelX = dimensions.X * 4;
            int bPixelY = dimensions.Y * 4;
            int bytesPerPixel = 4;

            // ====================== ALPHA ===============================
            // Get the pixel position of the block. Each block is 4x4 pixels.
            byte[] alpha = new byte[8];
            alpha[0] = GetHighestAlpha(level, bPixelX, bPixelY, bytesPerPixel);
            alpha[1] = GetLowestAlpha(level, bPixelX, bPixelY, bytesPerPixel);

            // Interpolate alpha 2 - 7 values
            if (alpha[0] > alpha[1])
            {
                // 6 interpolated alpha[ values.
                alpha[2] = (byte)(6 / 7 * alpha[0] + 1 / 7 * alpha[1]); // bit code 010
                alpha[3] = (byte)(5 / 7 * alpha[0] + 2 / 7 * alpha[1]); // bit code 011
                alpha[4] = (byte)(4 / 7 * alpha[0] + 3 / 7 * alpha[1]); // bit code 100
                alpha[5] = (byte)(3 / 7 * alpha[0] + 4 / 7 * alpha[1]); // bit code 101
                alpha[6] = (byte)(2 / 7 * alpha[0] + 5 / 7 * alpha[1]); // bit code 110
                alpha[7] = (byte)(1 / 7 * alpha[0] + 6 / 7 * alpha[1]); // bit code 111
            }
            else
            {
                // 4 interpolated alpha[ values.
                alpha[2] = (byte)(4 / 5 * alpha[0] + 1 / 5 * alpha[1]); // bit code 010
                alpha[3] = (byte)(3 / 5 * alpha[0] + 2 / 5 * alpha[1]); // bit code 011
                alpha[4] = (byte)(2 / 5 * alpha[0] + 3 / 5 * alpha[1]); // bit code 100
                alpha[5] = (byte)(1 / 5 * alpha[0] + 4 / 5 * alpha[1]); // bit code 101
                alpha[6] = 0;                         // bit code 110
                alpha[7] = 255;                       // bit code 111
            }

            // Write values
            writer.Write(alpha[0]);
            writer.Write(alpha[1]);

            ulong alphaMask = 0;
            int alphaID = 0;

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

                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + 3; // Add 3 bytes to reach the alpha

                    // Test distance of each color table entry
                    for (int i = 0; i < alpha.Length; i++)
                    {
                        float dist = Math.Abs(alpha[i] - level.Data[b]);
                        if (dist < closest)
                        {
                            closest = dist;
                            closestID = i;
                        }
                    }

                    // Write the ID of the closest alpha value
                    alphaMask |= (ulong)closestID << (3 * alphaID);
                    alphaID++;
                }
            }

            // Write 6 bytes of alpha mask. 
            // Note: 16x 3-bit pixel indices = 48 bits (6 bytes).
            byte[] alphaBytes = BitConverter.GetBytes(alphaMask);
            for (int i = 0; i < 6; i++)
                writer.Write(alphaBytes[i]);

            //==================== COLOR ===================================
            CompressBC1ColorBlock(writer, level, bPixelX, bPixelY, bytesPerPixel, false, 0, dimensions);
        }

        private byte GetHighestAlpha(TextureData.Slice level, int blockPixelX, int blockPixelY, int bytesPerPixel)
        {
            int pitch = level.Width * 4;
            byte result = 0;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int pX = blockPixelX + x;
                    int pY = blockPixelY + y;

                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + 3; // Add 3 bytes to reach the alpha
                    if (level.Data[b] > result)
                        result = level.Data[b];
                }
            }

            return result;
        }

        private byte GetLowestAlpha(TextureData.Slice level, int blockPixelX, int blockPixelY, int bytesPerPixel)
        {
            int pitch = level.Width * 4;
            byte result = 255;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int pX = blockPixelX + x;
                    int pY = blockPixelY + y;

                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + 3; // Add 3 bytes to reach the alpha

                    if (level.Data[b] < result)
                        result = level.Data[b];
                }
            }

            return result;
        }
    }
}
