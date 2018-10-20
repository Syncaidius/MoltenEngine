using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC1Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC1_Typeless, GraphicsFormat.BC1_UNorm, GraphicsFormat.BC1_UNorm_SRgb };

        protected override void DecompressBlock(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight, byte[] output)
        {
            DDSColorTable table;
            DecompressColorTableBC1(imageReader, out table);

            // TODO use fixed pointer block here on output array.
            for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            {
                int py = (dimensions.Y << 2) + bpy;
                for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
                {
                    uint index = (table.data >> 2 * (4 * bpy + bpx)) & 0x03;
                    Color c = table.color[index];
                    c.A = (byte)((table.rawColor[0] <= table.rawColor[1]) && ((table.data & 0x03) == 0x03) ? 0 : 255);

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

        protected override void CompressBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int bPixelX = dimensions.X * DDSHelper.BLOCK_DIMENSIONS;
            int bPixelY = dimensions.Y * DDSHelper.BLOCK_DIMENSIONS;
            int bytesPerPixel = 4; // Uncompressed bytes-per-pixel
            CompressBC1ColorBlock(writer, level, bPixelX, bPixelY, bytesPerPixel, true, 255, dimensions);
        }
    }
}
