using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    /// <summary>A base class for DDS block readers.</summary>
    internal abstract class BCBlockParser
    {
        const int ONE_BIT_ALPHA_THRESHOLD = 10;

        protected abstract void DecodeBlock(BinaryReader reader, BCDimensions dimensions, int levelWidth, int levelHeight, byte[] output);

        protected abstract void EncodeBlock(BinaryWriter writer, BCDimensions dimensions, TextureData.Slice level);

        public abstract GraphicsFormat[] SupportedFormats { get; }

        public byte[] Decode(TextureData.Slice level)
        {            
            // Pass to stream-based overload
            byte[] result = new byte[level.Width * level.Height * 4];

            using (MemoryStream stream = new MemoryStream(level.Data))
            {
                using (BinaryReader imageReader = new BinaryReader(stream))
                {
                    int blockCountX = Math.Max(1, (level.Width + 3) / DDSHelper.BLOCK_DIMENSIONS);
                    int blockCountY = Math.Max(1, (level.Height + 3) / DDSHelper.BLOCK_DIMENSIONS);

                    BCDimensions dimensions = new BCDimensions()
                    {
                        Width = Math.Min(level.Width, DDSHelper.BLOCK_DIMENSIONS),
                        Height = Math.Min(level.Height, DDSHelper.BLOCK_DIMENSIONS),
                    };

                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        dimensions.Y = blockY;
                        dimensions.PixelY = blockY * DDSHelper.BLOCK_DIMENSIONS;
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {
                            dimensions.X = blockX;
                            dimensions.PixelX = blockX * DDSHelper.BLOCK_DIMENSIONS;
                            DecodeBlock(imageReader, dimensions, level.Width, level.Height, result);
                        }
                    }
                }
            }

            return result;
        }

        public byte[] Encode(TextureData.Slice level)
        {
            int blockCountX = Math.Max(1, (level.Width + 3) / DDSHelper.BLOCK_DIMENSIONS);
            int blockCountY = Math.Max(1, (level.Height + 3) / DDSHelper.BLOCK_DIMENSIONS);
            byte[] result = null;

            BCDimensions dimensions = new BCDimensions()
            {
                Width = Math.Min(level.Width, DDSHelper.BLOCK_DIMENSIONS),
                Height = Math.Min(level.Height, DDSHelper.BLOCK_DIMENSIONS),
            };

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        dimensions.Y = blockY;
                        dimensions.PixelY = blockY * DDSHelper.BLOCK_DIMENSIONS;
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {
                            dimensions.X = blockX;
                            dimensions.PixelX = blockX * DDSHelper.BLOCK_DIMENSIONS;
                            EncodeBlock(writer, dimensions, level);
                        }
                    }

                    result = stream.ToArray();
                }
            }

            return result;
        }

        /// <summary>Reads and decompresses the color table from the provided reader.</summary>
        /// <param name="reader">The reader to use for retrieving the compressed data.</param>
        /// <param name="table">The destination for the decompressed color table.</param>
        /// <returns></returns>
        protected void DecodeColorTableBC1(BinaryReader reader, out BCColorTable table)
        {
            table = new BCColorTable();
            table.color = new Color[4];
            table.rawColor = new ushort[2];

            table.rawColor[0] = reader.ReadUInt16();
            table.rawColor[1] = reader.ReadUInt16();

            Convert565to888(table.rawColor[0], out table.color[0].R, out table.color[0].G, out table.color[0].B);
            Convert565to888(table.rawColor[1], out table.color[1].R, out table.color[1].G, out table.color[1].B);

            table.color[2] = LerpColorThird(table.color[0], table.color[1], 2, 1);
            table.color[3] = LerpColorThird(table.color[0], table.color[1], 1, 2);
            table.data = reader.ReadUInt32();
        }

        /// <summary>Writes the data of a block of pixels using DXT1 block-compression.</summary>
        /// <param name="writer">The binary writer to in which to feed the result.</param>
        /// <param name="level">The mip map level to compress.</param>
        /// <param name="bPixelX">The X pixel coordinate of the current block's top left corner.</param>
        /// <param name="bPixelY">The Y pixel coordinate of the current block's top left corner.</param>
        /// <param name="pixelByteSize">The size of a single pixel, in bytes.</param>
        protected void EncodeBC1ColorBlock(BinaryWriter writer, TextureData.Slice level, int bPixelX, int bPixelY, int pixelByteSize, bool oneBitAlpha, byte alphaThreshold, BCDimensions dimensions)
        {
            // Get the min and max color
            Color[] c = new Color[4];
            c[0] = GetClosestColor(new Color(0, 0, 0, 0), level, bPixelX, bPixelY, pixelByteSize, dimensions.Width, dimensions.Height);
            c[1] = GetClosestColor(new Color(255, 255, 255, 0), level, bPixelX, bPixelY, pixelByteSize, dimensions.Width, dimensions.Height);

            ushort packed0 = Convert888to565(c[0]);
            ushort packed1 = Convert888to565(c[1]);

            // Check for 1-bit alpha mode + pass.
            if (oneBitAlpha && (packed0 < packed1))
            {
                c[2] = (1f / 2f * c[0]) + (1f / 2f * c[1]);
                c[3] = Color.Transparent;
            }
            else
            {
                c[2] = LerpColorThird(c[0], c[1], 2, 1);
                c[3] = LerpColorThird(c[0], c[1], 1, 2);
            }

            writer.Write(packed0);
            writer.Write(packed1);

            uint dataTable = 0;
            int colorID = 0;
            for (int y = 0; y < dimensions.Height; y++)
            {
                for (int x = 0; x < dimensions.Width; x++)
                {
                    float closest = float.MaxValue;
                    uint closestID = 3;

                    int pX = bPixelX + x;
                    int pY = bPixelY + y;
                    int b = GetPixelFirstByte(pX, pY, level.Width, pixelByteSize);

                    Color col = new Color()
                    {
                        R = level.Data[b],
                        G = level.Data[b + 1],
                        B = level.Data[b + 2],
                        A = level.Data[b + 3],
                    };

                    if (col.A < alphaThreshold)
                    {
                        closestID = 3;
                    }
                    else
                    {
                        // Only test against the first 3 colors.
                        for (uint i = 0; i < 3; i++)
                        {
                            float dist = ColorDistance(c[i], col);

                            if (dist < closest)
                            {
                                closest = dist;
                                closestID = i;
                            }
                        }
                    }

                    // Write closest ID to color table
                    dataTable |= closestID << (2 * colorID);
                    colorID++;
                }
            }

            // Write color table
            writer.Write(dataTable);
        }

        protected void Convert565to888(ushort color, out byte r, out byte g, out byte b)
        {
            int iR = (color >> 11) & 0x1f;
            int iG = (color >> 5) & 0x3f;
            int iB = (color) & 0x1f;

            // Align values to the 8th bit, then move back to average
            r = (byte)((iR << 3) | (iR >> 2));  // move 3 bits forward to align to 8th bit (value is only 5 bits)
            g = (byte)((iG << 2) | (iG >> 4));  // move 2 bits forward to align to 8th bit (value is only 6 bits)
            b = (byte)((iB << 3) | (iB >> 2));  // move 3 bits forward to align to 8th bit (value is only 5 bits)
        }

        protected ushort Convert888to565(Color color)
        {
            ushort r = (ushort)(Mul8Bit(color.R, 31) << 11);
            ushort g = (ushort)(Mul8Bit(color.G, 63) << 5);
            ushort b = (ushort)Mul8Bit(color.B, 31);
            return (ushort)(r | g | b);
        }

        protected Color LerpColorThird(Color c0, Color c1, int third0, int third1)
        {
            return new Color()
            {
                R = (byte)(third0 / 3f * c0.R + third1 / 3f * c1.R),
                G = (byte)(third0 / 3f * c0.G + third1 / 3f * c1.G),
                B = (byte)(third0 / 3f * c0.B + third1 / 3f * c1.B),
            };
        }

        private Color GetClosestColor(Color target, TextureData.Slice level, int blockPixelX, int blockPixelY, int colorByteSize, int blockWidth, int blockHeight)
        {
            int pitch = level.Width * 4;
            Color result = new Color(255, 255, 255, 0);
            float closest = uint.MaxValue;

            for (int y = 0; y < blockHeight; y++)
            {
                int pY = blockPixelY + y;
                for (int x = 0; x < blockWidth; x++)
                {
                    int pX = blockPixelX + x;
                    int offset = GetPixelFirstByte(pX, pY, level.Width, colorByteSize);
                    Color color = new Color()
                    {
                        R = level.Data[offset],
                        G = level.Data[offset + 1],
                        B = level.Data[offset + 2],
                        A = 0,
                    };

                    // Check if the packed version of the color is lower than the current lowest.
                    float dist = ColorDistance(target, color);
                    if (dist < closest)
                    {
                        closest = dist;
                        result = color;
                    }
                }
            }

            return result;
        }

        private float ColorDistance(Color value1, Color value2)
        {
            // TODO pack into single int and compare distance with that instead.
            //      This will allow the costly square root to be removed.
            float x = value1.R - value2.R;
            float y = value1.G - value2.G;
            float z = value1.B - value2.B;

            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Gets the first byte of a pixel within an image.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="imageWidth"></param>
        /// <param name="bytesPerPixel">The number of bytes per pixel. For example, RGBA is 4 bytes, a single-channel is 1 byte.</param>
        /// <returns></returns>
        protected int GetPixelFirstByte(int x, int y, int imageWidth, int bytesPerPixel)
        {
            int pitch = (imageWidth * bytesPerPixel);
            return (pitch * y) + (x * bytesPerPixel);
        }

        private int Mul8Bit(int a, int b)
        {
            int t = a * b + 128;
            return (t + (t >> 8)) >> 8;
        }

        private void GetHighLowSingleChannel(TextureData.Slice level, ref BCDimensions dimensions, int bytesPerPixel, int valueByteOffset, out byte lowest, out byte highest)
        {
            lowest = 255;
            highest = 0;

            for (int bpy = 0; bpy < dimensions.Height; bpy++)
            {
                int pY = dimensions.PixelY + bpy;
                for (int bpx = 0; bpx < dimensions.Width; bpx++)
                {
                    int pX = dimensions.PixelX + bpx;
                    int offset = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + valueByteOffset;

                    if (level.Data[offset] < lowest)
                        lowest = level.Data[offset];
                    else if (level.Data[offset] > highest)
                        highest = level.Data[offset];
                }
            }
        }

        /// <summary>
        /// Finds the highest and lowest colors to store at index 0 and 1. Populates the rest of the table by interpolating between the first two colors.
        /// </summary>
        /// <param name="level">The current texture slice.</param>
        /// <param name="dimensions">The dimensions of the current BC block.</param>
        /// <param name="bytesPerPixel">The number of bytes-per-pixel when uncompressed.</param>
        /// <param name="valueByteOffset">The number of bytes to offset from the start of a pixel in order to reach the value we want.</param>
        /// <returns></returns>
        protected byte[] GetColorTableSingleChannel(TextureData.Slice level, ref BCDimensions dimensions, int bytesPerPixel, int valueByteOffset)
        {
            byte[] val = new byte[8];
            GetHighLowSingleChannel(level, ref dimensions, bytesPerPixel, valueByteOffset, out val[0], out val[1]);

            // Interpolate value 2 - 7 values
            if (val[0] > val[1])
            {
                // 6 interpolated color values
                val[2] = (byte)((6 * val[0] + 1 * val[1]) / 7f);  // bit code 010
                val[3] = (byte)((5 * val[0] + 2 * val[1]) / 7f);  // bit code 011
                val[4] = (byte)((4 * val[0] + 3 * val[1]) / 7f);  // bit code 100
                val[5] = (byte)((3 * val[0] + 4 * val[1]) / 7f);  // bit code 101
                val[6] = (byte)((2 * val[0] + 5 * val[1]) / 7f);  // bit code 110
                val[7] = (byte)((1 * val[0] + 6 * val[1]) / 7f);  // bit code 111
            }
            else
            {
                // 4 interpolated color values
                val[2] = (byte)((4 * val[0] + 1 * val[1]) / 5f);  // bit code 010
                val[3] = (byte)((3 * val[0] + 2 * val[1]) / 5f);  // bit code 011
                val[4] = (byte)((2 * val[0] + 3 * val[1]) / 5f);  // bit code 100
                val[5] = (byte)((1 * val[0] + 4 * val[1]) / 5f);  // bit code 101
                val[6] = 0;                              // bit code 110
                val[7] = 255;                            // bit code 111
            }

            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="level"></param>
        /// <param name="dimensions"></param>
        /// <param name="bytesPerPixel"></param>
        /// <param name="valueByteOffset">The number of bytes to offset from the start of a pixel in order to reach the value we want.</param>
        protected void Encode8BitSingleChannelBlock(BinaryWriter writer, TextureData.Slice level, ref BCDimensions dimensions, int bytesPerPixel, int valueByteOffset)
        {
            byte[] table = GetColorTableSingleChannel(level, ref dimensions, bytesPerPixel, valueByteOffset);

            // Write values
            writer.Write(table[0]);
            writer.Write(table[1]);

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
                    int b = GetPixelFirstByte(pX, pY, level.Width, bytesPerPixel) + valueByteOffset;

                    // Test distance of each color table entry
                    for (uint i = 0; i < table.Length; i++)
                    {
                        float dist = Math.Abs(table[i] - level.Data[b]);
                        if (dist < closest)
                        {
                            closest = dist;
                            closestID = i;
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
            byte[] bytes = BitConverter.GetBytes(mask);
            writer.Write(bytes, 0, 6);
        }

        protected ulong Decode8BitSingleChannelMask(BinaryReader reader, out byte lowest, out byte highest)
        {
            lowest = reader.ReadByte();
            highest = reader.ReadByte();

            // Read each of the sixteen 3-bit pixel indices, totaling 48 bits (6 bytes)
            ulong mask = reader.ReadByte();
            mask += (ulong)reader.ReadByte() << 8;
            mask += (ulong)reader.ReadByte() << 16;
            mask += (ulong)reader.ReadByte() << 24;
            mask += (ulong)reader.ReadByte() << 32;
            mask += (ulong)reader.ReadByte() << 40;
            return mask;
        }

        protected byte DecodeSingleChannelColor(ulong mask, int bpX, int bpY, byte col0, byte col1)
        {
            ulong index = ((mask >> 3 * (DDSHelper.BLOCK_DIMENSIONS * bpY + bpX)) & 0x07);

            if (index == 0)
                return col0;
            else if (index == 1)
                return col1;
            else if (col0 > col1)
                return (byte)((index * col0 + index * col1) / 7.0f);
            else if (index == 6)
                return 0;
            else if (index == 7)
                return 255;
            else
                return (byte)((index * col0 + index * col1) / 5.0f);
        }
    }
}
