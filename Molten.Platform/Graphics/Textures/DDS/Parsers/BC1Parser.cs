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

        protected override void DecodeBlock(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight, byte[] output)
        {
            D3DX_BC1 bc1 = new D3DX_BC1();
            bc1.Read(imageReader);
            Color4[] colTable = BC.D3DXDecodeBC1(bc1);

            // TODO use fixed pointer block here on output array.
            int index = 0;
            for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            {
                int py = (dimensions.Y << 2) + bpy;
                for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
                {
                    Color c = (Color)colTable[index++];

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

            //BCColorTable table;
            //DecodeColorTableBC1(imageReader, out table);

            //// TODO use fixed pointer block here on output array.
            //for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            //{
            //    int py = (dimensions.Y << 2) + bpy;
            //    for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
            //    {
            //        uint index = (table.data >> 2 * (4 * bpy + bpx)) & 0x03;
            //        Color c = table.color[index];

            //        int px = (dimensions.X << 2) + bpx;
            //        if ((px < levelWidth) && (py < levelHeight))
            //        {
            //            int offset = ((py * levelWidth) + px) << 2;
            //            output[offset] = c.R;
            //            output[offset + 1] = c.G;
            //            output[offset + 2] = c.B;
            //            output[offset + 3] = (byte)((table.rawColor[0] <= table.rawColor[1]) && ((table.data & 0x03) == 0x03) ? 0 : 255);
            //        }
            //    }
            //}
        }

        protected override void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
        {
            int index = 0;
            Color4[] colTable = new Color4[BC.NUM_PIXELS_PER_BLOCK];

            for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
            {
                int py = (dimensions.Y << 2) + bpy;
                for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
                {
                    int px = (dimensions.X << 2) + bpx;
                    if ((px < level.Width) && (py < level.Height))
                    {
                        int offset = ((py * level.Width) + px) << 2;
                        colTable[index++] = new Color()
                        {
                            R = level.Data[offset],
                            G = level.Data[offset + 1],
                            B = level.Data[offset + 2],
                            A = level.Data[offset + 3]
                        };
                    }
                }
            }

            D3DX_BC1 bc1 = BC.D3DXEncodeBC1(colTable, 1.0f, BCFlags.NONE);
            bc1.Write(writer);

            //int bPixelX = dimensions.X * DDSHelper.BLOCK_DIMENSIONS;
            //int bPixelY = dimensions.Y * DDSHelper.BLOCK_DIMENSIONS;
            //int bytesPerPixel = 4; // Uncompressed bytes-per-pixel
            //EncodeBC1ColorBlock(writer, level, bPixelX, bPixelY, bytesPerPixel, true, 255, dimensions);
        }
    }
}
