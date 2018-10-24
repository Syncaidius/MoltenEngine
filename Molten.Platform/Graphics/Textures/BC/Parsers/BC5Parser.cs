//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace Molten.Graphics.Textures
//{
//    internal class BC5Parser : BCBlockParser
//    {
//        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC5_UNorm };

//        protected unsafe override void DecodeBlock(BinaryReader reader, BCDimensions dimensions, int width, int height, byte[] output)
//        {
//            byte red0;
//            byte red1;
//            ulong redMask = Decode8BitSingleChannelMask(reader, out red0, out red1);

//            byte green0;
//            byte green1;
//            ulong greenMask = Decode8BitSingleChannelMask(reader, out green0, out green1);

//            // Decompress pixel data from block
//            for (int bpy = 0; bpy < DDSHelper.BLOCK_DIMENSIONS; bpy++)
//            {
//                int py = (dimensions.Y << 2) + bpy;
//                for (int bpx = 0; bpx < DDSHelper.BLOCK_DIMENSIONS; bpx++)
//                {
//                    int px = (dimensions.X << 2) + bpx;
//                    if ((px < width) && (py < height))
//                    {
//                        int offset = ((py * width) + px) << 2;
//                        output[offset] = DecodeSingleChannelColor(redMask, bpx, bpy, red0, red1);
//                        output[offset + 1] = DecodeSingleChannelColor(greenMask, bpx, bpy, green0, green1);
//                        output[offset + 2] = 0;
//                        output[offset + 3] = 255;
//                    }
//                }
//            }
//        }

//        protected override void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level)
//        {
//            int bytesPerPixel = 4;
//            Encode8BitSingleChannelBlock(writer, level, ref dimensions, bytesPerPixel, 0); // Encode red channel
//            Encode8BitSingleChannelBlock(writer, level, ref dimensions, bytesPerPixel, 1); // Encode green channel.
//        }
//    }
//}
