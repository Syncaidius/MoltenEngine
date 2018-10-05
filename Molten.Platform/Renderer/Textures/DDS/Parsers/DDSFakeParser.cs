using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures.DDS.Parsers
{
    internal class DDSFakeParser : DDSBlockParser
    {
        Color _fakeColor = new Color(255, 0, 220, 255);

        int _curCol = 0;

        protected override void DecompressBlock(BinaryReader imageReader, int x, int y, int width, int height, byte[] output)
        {
            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        output[offset] = _fakeColor.R;
                        output[offset + 1] = _fakeColor.G;
                        output[offset + 2] = _fakeColor.B;
                        output[offset + 3] = _fakeColor.A;
                    }
                }
            }
        }

        protected override void CompressBlock(BinaryWriter writer, int x, int y, TextureData.Slice level)
        {
            throw new NotImplementedException();
        }
    }
}
