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

        protected override void DecodeBlock(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight, byte[] output)
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
            DecodeColorTableBC1(imageReader, out table);

            int alphaIndex = 0;
            for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            {
                int py = (dimensions.Y << 2) + bpy;

                for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
                {
                    uint index = (table.data >> 2 * (4 * bpy + bpx)) & 0x03;
                    Color c = table.color[index];

                    switch (alphaIndex)
                    {
                        case 0: c.A = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4)); break;
                        case 1: c.A = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4)); break;
                        case 2: c.A = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4)); break;
                        case 3: c.A = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4)); break;
                        case 4: c.A = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4)); break;
                        case 5: c.A = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4)); break;
                        case 6: c.A = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4)); break;
                        case 7: c.A = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4)); break;
                        case 8: c.A = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4)); break;
                        case 9: c.A = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4)); break;
                        case 10: c.A = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4)); break;
                        case 11: c.A = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4)); break;
                        case 12: c.A = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4)); break;
                        case 13: c.A = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4)); break;
                        case 14: c.A = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4)); break;
                        case 15: c.A = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4)); break;
                    }
                    alphaIndex++;

                    // Store decompressed color data.
                    int px = (dimensions.X << 2) + bpx;
                    if ((px < levelWidth) && (py < levelHeight))
                    {
                        int offset = ((py * levelWidth) + px) << 2;
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
            // Get the pixel position of the block. Each block is 4x4 pixels.
            int bPixelX = dimensions.X * DDSHelper.BLOCK_DIMENSIONS;
            int bPixelY = dimensions.Y * DDSHelper.BLOCK_DIMENSIONS;
            int bytesPerPixel = 4; // Number of uncompressed bytes per pixel.

            int bpy = 0;
            int bpx = 0;
            byte zero = 0;

            // Since alpha is written in 4-bit pairs (2 pixel alphas per byte), We need a minimum width of 2 for this to work correctly.
            if(dimensions.Width < 2)
            {
                for (; bpy < dimensions.Height; bpy++)
                {
                    int pY = bPixelY + bpy;
                    int offset = GetPixelFirstByte(bPixelX, pY, level.Width, bytesPerPixel) + 3; // Add 3 bytes to access alpha
                    byte a1 = level.Data[offset];
                    byte result = (byte)(((a1 >> 4) << 4) | (a1 >> 4)); // We only have 1 pixel to work with, so use the same alpha for 4 halves of the byte.
                    writer.Write(result);
                    writer.Write(zero); // Fill the other half of the row with zero.
                }
            }
            else
            {
                for (; bpy < dimensions.Height; bpy++)
                {
                    int pY = bPixelY + bpy;
                    for (bpx = 0; bpx < dimensions.Width; bpx += 2) // Increment by 2 pixels each iteration.
                    {
                        int pX = bPixelX + bpx;
                        int offset = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + 3; // Add 3 bytes to access alpha

                        byte a1 = level.Data[offset];
                        byte a2 = level.Data[offset + 4];
                        byte result = (byte)(((a2 >> 4) << 4) | (a1 >> 4));
                        writer.Write(result);
                    }

                    // Fill the rest of the row with zero, if needed.
                    if (bpx < DDSHelper.BLOCK_DIMENSIONS)
                        writer.Write(zero);
                }
            }

            // Add the remaining rows, if any
            for (; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            {
                writer.Write(zero);
                writer.Write(zero);
            }


            // Write color data
            EncodeBC1ColorBlock(writer, level, bPixelX, bPixelY, bytesPerPixel, false, 0, dimensions);
        }
    }
}
