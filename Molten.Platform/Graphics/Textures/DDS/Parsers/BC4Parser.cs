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

            byte[] red = new byte[8];
            GetHighLow(level, ref dimensions, bytesPerPixel, out red[0], out red[1]);

            // Interpolate alpha 2 - 7 values
            if (red[0] > red[1])
            {
                // 6 interpolated color values
                red[2] = (byte)((6 * red[0] + 1 * red[1]) / 7f);  // bit code 010
                red[3] = (byte)((5 * red[0] + 2 * red[1]) / 7f);  // bit code 011
                red[4] = (byte)((4 * red[0] + 3 * red[1]) / 7f);  // bit code 100
                red[5] = (byte)((3 * red[0] + 4 * red[1]) / 7f);  // bit code 101
                red[6] = (byte)((2 * red[0] + 5 * red[1]) / 7f);  // bit code 110
                red[7] = (byte)((1 * red[0] + 6 * red[1]) / 7f);  // bit code 111
            }
            else
            {
                // 4 interpolated color values
                red[2] = (byte)((4 * red[0] + 1 * red[1]) / 5f);  // bit code 010
                red[3] = (byte)((3 * red[0] + 2 * red[1]) / 5f);  // bit code 011
                red[4] = (byte)((2 * red[0] + 3 * red[1]) / 5f);  // bit code 100
                red[5] = (byte)((1 * red[0] + 4 * red[1]) / 5f);  // bit code 101
                red[6] = 0;                              // bit code 110
                red[7] = 255;                            // bit code 111
            }

            writer.Write(red[0]);
            writer.Write(red[1]);

            ulong redMask = 0;
            int redID = 0;

            if(dimensions.Width < 4)
            {
                int derp = 0;
            }
            // Build 6-byte alpha mask by generating 16x 3-bit pixel indices.
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
                    for (uint i = 0; i < red.Length; i++)
                    {
                        float dist = Math.Abs(red[i] - level.Data[b]);
                        if (dist < closest)
                        {
                            closest = dist;
                            closestID = i;
                            if (dist == 0)
                                break;
                        }
                    }

                    // Write the ID of the closest alpha value
                    redMask |= closestID << (3 * redID);
                    redID++;
                }

                // Skip the remaining IDs within the current row.
                redID += DDSHelper.BLOCK_DIMENSIONS - bpx;
            }

            // Write 6 bytes of red mask. 
            // Note: 16x 3-bit pixel indices = 48 bits (6 bytes).
            byte[] redBytes = BitConverter.GetBytes(redMask);
            writer.Write(redBytes, 0, 6);
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
                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel);

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
