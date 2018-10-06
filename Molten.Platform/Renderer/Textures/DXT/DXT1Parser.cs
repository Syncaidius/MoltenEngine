using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class DXT1Parser : DXTBlockParser
    {
        protected override void DecompressBlock(BinaryReader imageReader, int x, int y, int width, int height, byte[] output)
        {
            DXTColorTable table;
            DecompressColorTableDXT1(imageReader, out table);

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    Color c = new Color(0, 0, 0, 255);

                    c.A = (byte)((table.rawColor[0] <= table.rawColor[1]) && ((table.data & 0x03) == 0x03) ? 0 : 255);

                    uint index = (table.data >> 2 * (4 * blockY + blockX)) & 0x03;

                    c = table.color[index];

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
            int bPixelX = bX * 4;
            int bPixelY = bY * 4;
            int pixelByteSize = 4;
            CompressDXT1Block(writer, level, bPixelX, bPixelY, pixelByteSize, true, 255);
        }
    }
}
