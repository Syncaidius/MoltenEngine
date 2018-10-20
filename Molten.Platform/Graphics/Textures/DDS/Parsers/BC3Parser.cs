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
            DDSColorTable table;
            DecodeColorTableBC1(reader, out table);

            // Decompress pixel data from block
            for (int pY = 0; pY < DDSHelper.BLOCK_DIMENSIONS; pY++)
            {
                int py = (dimensions.Y << 2) + pY;

                for (int pX = 0; pX < DDSHelper.BLOCK_DIMENSIONS; pX++)
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
                        c.A = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7f);
                    else if (alphaIndex == 6)
                        c.A = 0;
                    else if (alphaIndex == 7)
                        c.A = 255;
                    else
                        c.A = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5f);

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

        protected override void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int bytesPerPixel = 4;

            // ====================== ALPHA ===============================
            // Get the pixel position of the block. Each block is 4x4 pixels.
            byte[] alpha = new byte[8];
            GetHighLow(level, ref dimensions, bytesPerPixel, out alpha[0], out alpha[1]);

            // Interpolate alpha 2 - 7 values
            if (alpha[0] > alpha[1])
            {
                // 6 interpolated color values
                alpha[2] = (byte)((6 * alpha[0] + 1 * alpha[1]) / 7f);  // bit code 010
                alpha[3] = (byte)((5 * alpha[0] + 2 * alpha[1]) / 7f);  // bit code 011
                alpha[4] = (byte)((4 * alpha[0] + 3 * alpha[1]) / 7f);  // bit code 100
                alpha[5] = (byte)((3 * alpha[0] + 4 * alpha[1]) / 7f);  // bit code 101
                alpha[6] = (byte)((2 * alpha[0] + 5 * alpha[1]) / 7f);  // bit code 110
                alpha[7] = (byte)((1 * alpha[0] + 6 * alpha[1]) / 7f);  // bit code 111
            }
            else
            {
                // 4 interpolated color values
                alpha[2] = (byte)((4 * alpha[0] + 1 * alpha[1]) / 5f);  // bit code 010
                alpha[3] = (byte)((3 * alpha[0] + 2 * alpha[1]) / 5f);  // bit code 011
                alpha[4] = (byte)((2 * alpha[0] + 3 * alpha[1]) / 5f);  // bit code 100
                alpha[5] = (byte)((1 * alpha[0] + 4 * alpha[1]) / 5f);  // bit code 101
                alpha[6] = 0;                              // bit code 110
                alpha[7] = 255;                            // bit code 111
            }

            // Write values
            writer.Write(alpha[0]);
            writer.Write(alpha[1]);

            ulong mask = 0;
            int maskID = 0;

            // Build 6-byte red mask by generating 16x 3-bit pixel indices.
            // Note: 16x 3-bit pixel indices = 48 bits (6 bytes).
            int bpy = 0;
            for (; bpy < dimensions.Height; bpy++)
            {
                int pY = dimensions.PixelY + bpy;
                int bpx = 0;
                for (; bpx < dimensions.Width; bpx++)
                {
                    float closest = float.MaxValue;
                    ulong closestID = 7;
                    int pX = dimensions.PixelX + bpx;
                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel);

                    // Test distance of each color table entry
                    for (uint i = 0; i < alpha.Length; i++)
                    {
                        float dist = Math.Abs(alpha[i] - level.Data[b]);
                        if (dist < closest)
                        {
                            closest = dist;
                            closestID = i;
                            if (dist == 0)
                                break;
                        }
                    }

                    // Write the ID of the closest alpha value
                    mask |= closestID << (3 * maskID);
                    maskID++;
                }

                // Skip the remaining IDs within the current row.
                maskID += DDSHelper.BLOCK_DIMENSIONS - bpx;
            }

            // Write 6 bytes of alpha mask. 
            // Note: 16x 3-bit pixel indices = 48 bits (6 bytes).
            byte[] redBytes = BitConverter.GetBytes(mask);
            writer.Write(redBytes, 0, 6);

            //==================== COLOR ===================================
            EncodeBC1ColorBlock(writer, level, dimensions.PixelX, dimensions.PixelY, bytesPerPixel, false, 0, dimensions);
        }

        private void GetHighLow(TextureData.Slice level, ref BCDimensions dimensions, int bytesPerPixel, out byte lowest, out byte highest)
        {
            lowest = 255;
            highest = 0;

            for (int bpy = 0; bpy < dimensions.Height; bpy++)
            {
                int pY = dimensions.PixelY + bpy;
                for (int bpx = 0; bpx < dimensions.Width; bpx++)
                {
                    int pX = dimensions.PixelX + bpx;
                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + 3;  // Add 3 bytes to reach the alpha

                    if (level.Data[b] < lowest)
                        lowest = level.Data[b];
                    else
                        if (level.Data[b] > highest)
                            highest = level.Data[b];
                }
            }
        }
    }
}
