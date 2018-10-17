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

        protected abstract void DecompressBlock(BinaryReader reader, int x, int y, int width, int height, byte[] output);

        protected abstract void CompressBlock(BinaryWriter writer, int x, int y, TextureData.Slice level);

        public abstract GraphicsFormat[] SupportedFormats { get; }

        public byte[] Decompress(TextureData.Slice level)
        {
            MemoryStream stream = new MemoryStream(level.Data);

            // Pass to stream-based overload
            byte[] result = new byte[level.Width * level.Height * 4];

            using (BinaryReader imageReader = new BinaryReader(stream))
            {
                int blockCountX = Math.Max(1, (level.Width + 3) / 4);
                int blockCountY = Math.Max(1, (level.Height + 3) / 4);

                for (int y = 0; y < blockCountY; y++)
                    for (int x = 0; x < blockCountX; x++)
                        DecompressBlock(imageReader, x, y, level.Width, level.Height, result);
            }

            stream.Dispose();

            return result;
        }

        public byte[] Compress(TextureData.Slice level)
        {
            MemoryStream stream = new MemoryStream();

            int blockCountX = Math.Max(1, (level.Width + 3) / 4);
            int blockCountY = Math.Max(1, (level.Height + 3) / 4);
            byte[] result = null;

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (int y = 0; y < blockCountY; y++)
                    for (int x = 0; x < blockCountX; x++)
                        CompressBlock(writer, x, y, level);

                // Retrieve written data.
                result = stream.ToArray();
            }

            stream.Dispose();

            return result;
        }

        /// <summary>Reads and decompresses the color table from the provided reader.</summary>
        /// <param name="reader">The reader to use for retrieving the compressed data.</param>
        /// <param name="table">The destination for the decompressed color table.</param>
        /// <returns></returns>
        protected DDSColorTable DecompressColorTableDXT1(BinaryReader reader, out DDSColorTable table)
        {
            table = new DDSColorTable();
            table.color = new Color[4];
            table.rawColor = new ushort[2];

            table.rawColor[0] = reader.ReadUInt16();
            table.rawColor[1] = reader.ReadUInt16();

            Convert565to888(table.rawColor[0], out table.color[0].R, out table.color[0].G, out table.color[0].B);
            Convert565to888(table.rawColor[1], out table.color[1].R, out table.color[1].G, out table.color[1].B);

            table.color[2] = LerpColorThird(table.color[0], table.color[1], 2, 1);
            table.color[3] = LerpColorThird(table.color[0], table.color[1], 1, 2);

            table.data = reader.ReadUInt32();

            return table;
        }

        /// <summary>Writes the data of a block of pixels using DXT1 block-compression.</summary>
        /// <param name="writer">The binary writer to in which to feed the result.</param>
        /// <param name="level">The mip map level to compress.</param>
        /// <param name="bPixelX">The X pixel coordinate of the current block's top left corner.</param>
        /// <param name="bPixelY">The Y pixel coordinate of the current block's top left corner.</param>
        /// <param name="pixelByteSize">The size of a single pixel, in bytes.</param>
        protected void CompressDXT1Block(BinaryWriter writer, TextureData.Slice level, int bPixelX, int bPixelY, int pixelByteSize, bool oneBitAlpha, byte alphaThreshold)
        {
            // Get the min and max color
            Color[] c = new Color[4];
            c[0] = GetClosestColor(new Color(0, 0, 0, 0), level, bPixelX, bPixelY, pixelByteSize);
            c[1] = GetClosestColor(new Color(255, 255, 255, 0), level, bPixelX, bPixelY, pixelByteSize);

            ushort packed0 = Convert888to565(c[0]);
            ushort packed1 = Convert888to565(c[1]);

            // Check for 1-bit alpha mode + pass.
            if (oneBitAlpha && (packed0 < packed1))
            {
                c[2] = (1f / 2f * c[0]) + (1f / 2f * c[1]);
                c[3] = new Color(0, 0, 0, 0);
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
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    float closest = float.MaxValue;
                    uint closestID = 3;

                    int pX = bPixelX + x;
                    int pY = bPixelY + y;
                    int b = GetPixelByte(pX, pY, level.Width, pixelByteSize);

                    Color col = new Color()
                    {
                        R = level.Data[b],
                        G = level.Data[b+1],
                        B = level.Data[b+2],
                        A = level.Data[b+3],
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
            int iG = (color >>  5) & 0x3f;
            int iB = (color) & 0x1f;

            // Align values to the 8th bit, then move back to average
            r = (byte)((iR << 3) | (iR >> 2));  // move 3 bits forward to align to 8th bit (value is only 5 bits)
            g = (byte)((iG << 2) | (iG >> 4));  // move 2 bits forward to align to 8th bit (value is only 6 bits)
            b = (byte)((iB << 3) | (iB >> 2));  // move 3 bits forward to align to 8th bit (value is only 5 bits)
        }

        protected ushort Convert888to565(Color color)
        {
            ushort r = (ushort)(Mul8Bit(color.R, 31) << 11);
            ushort g = (ushort)(Mul8Bit(color.G, 63) <<  5);
            ushort b = (ushort)Mul8Bit(color.B, 31);
            return (ushort)(r | g | b);
        }

        protected Color LerpColorThird(Color c0, Color c1, int third0, int third1)
        {
            return new Color()
            {
                R = (byte)((third0 / 3f * c0.R) + (third1 / 3f * c1.R)),
                G = (byte)((third0 / 3f * c0.G) + (third1 / 3f * c1.G)),
                B = (byte)((third0 / 3f * c0.B) + (third1 / 3f * c1.B)),
            };
        }

        private Color GetClosestColor(Color target, TextureData.Slice level, int blockPixelX, int blockPixelY, int colorByteSize)
        {
            int pitch = level.Width * 4;

            Color result = new Color(255, 255, 255, 0);
            float closest = uint.MaxValue;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int pX = blockPixelX + x;
                    int pY = blockPixelY + y;

                    int b = GetPixelByte(pX, pY, level.Width, colorByteSize);
                    Color color = new Color()
                    {
                        R = level.Data[b],
                        G = level.Data[b+1],
                        B = level.Data[b+2],
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
            float x = value1.R - value2.R;
            float y = value1.G - value2.G;
            float z = value1.B - value2.B;

            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }


        protected int GetPixelByte(int x, int y, int imageWidth, int colorByteSize)
        {
            int pitch = (imageWidth * colorByteSize);
            return (pitch * y) + (x * colorByteSize);
        }

        int Mul8Bit(int a, int b)
    {
        int t = a * b + 128;
        return (t + (t >> 8)) >> 8;
    }
}
}
