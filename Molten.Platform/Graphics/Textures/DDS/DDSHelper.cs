using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    public static class DDSHelper
    {
        /// <summary>
        /// The expected width and height of a DDS data block, in pixels.
        /// </summary>
        public const int BLOCK_DIMENSIONS = 4;

        static Dictionary<GraphicsFormat, BCBlockParser> _parsers;

        static DDSHelper()
        {
            _parsers = new Dictionary<GraphicsFormat, BCBlockParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindType<BCBlockParser>();
            foreach (Type t in parserTypes)
            {
                BCBlockParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as BCBlockParser;
                _parsers.Add(parser.ExpectedFormat, parser);
            }
        }

        public static void Decompress(TextureData data, Logger log)
        {
            // Cannot decompress uncompressed data.
            if (data.IsCompressed == false)
                return;

            TextureData.Slice[] levels = data.Levels;
            data.Levels = new TextureData.Slice[levels.Length];

            BCBlockParser parser = null;

            if (_parsers.TryGetValue(data.Format, out parser))
            {
                for (int a = 0; a < data.ArraySize; a++)
                {
                    for (int i = 0; i < data.MipMapLevels; i++)
                    {
                        int levelID = (a * (int)data.MipMapLevels) + i;
                        byte[] decompressed = DecompressLevel(parser, levels[levelID], log);

                        data.Levels[levelID] = new TextureData.Slice()
                        {
                            Data = decompressed,
                            Height = levels[i].Height,
                            Width = levels[i].Width,
                            Pitch = levels[i].Width * 4,
                            TotalBytes = decompressed.Length,
                        };
                    }
                }

                data.Format = GraphicsFormat.R8G8B8A8_UNorm;
                data.IsCompressed = false;
            }
        }

        private static byte[] DecompressLevel(BCBlockParser parser, TextureData.Slice compressed, Logger log)
        {
            // Pass to stream-based overload
            byte[] result = new byte[compressed.Width * compressed.Height * 4];

            using (MemoryStream stream = new MemoryStream(compressed.Data))
            {
                using (BinaryReader imageReader = new BinaryReader(stream))
                {
                    int blockCountX = Math.Max(1, (compressed.Width + 3) / BLOCK_DIMENSIONS);
                    int blockCountY = Math.Max(1, (compressed.Height + 3) / BLOCK_DIMENSIONS);
                    int blockWidth = Math.Min(compressed.Width, BLOCK_DIMENSIONS);
                    int blockHeight = Math.Min(compressed.Height, BLOCK_DIMENSIONS);

                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {
                            Color4[] pixels = parser.Decode(imageReader, log);

                            // Transfer the decompressed pixel data into the image.
                            int index = 0;
                            for (int bpy = 0; bpy < BLOCK_DIMENSIONS; bpy++)
                            {
                                int py = (blockY << 2) + bpy;
                                for (int bpx = 0; bpx < BLOCK_DIMENSIONS; bpx++)
                                {
                                    Color c = (Color)pixels[index++];

                                    int px = (blockX << 2) + bpx;
                                    if ((px < compressed.Width) && (py < compressed.Height))
                                    {
                                        int offset = ((py * compressed.Width) + px) << 2;
                                        result[offset] = c.R;
                                        result[offset + 1] = c.G;
                                        result[offset + 2] = c.B;
                                        result[offset + 3] = c.A;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Compresses the texture data into DXT5 block-compressed format, if not already compressed.</summary>
        /// <param name="data"></param>
        public static void Compress(TextureData data, DDSFormat compressionFormat, Logger log)
        {
            // Cannot compress data which is already compressed.
            if (data.IsCompressed)
                return;

            if (data.Width % 4 > 0 || data.Height % 4 > 0)
                throw new DDSSizeException(compressionFormat, data.Width, data.Height);

            TextureData.Slice[] levels = data.Levels;
            data.Levels = new TextureData.Slice[levels.Length];
            GraphicsFormat gFormat = compressionFormat.ToGraphicsFormat();

            BCBlockParser parser = null;
            if (_parsers.TryGetValue(gFormat, out parser))
            {
                for (int a = 0; a < data.ArraySize; a++)
                {
                    for (int i = 0; i < data.MipMapLevels; i++)
                    {
                        int levelID = (a * (int)data.MipMapLevels) + i;
                        byte[] levelData = CompressLevel(parser, levels[levelID], log);
                        int pitch = Math.Max(1, ((levels[i].Width + 3) / 4) * DDSHelper.GetBlockSize(gFormat));

                        int blockCountY = (levels[i].Height + 3) / 4;

                        data.Levels[levelID] = new TextureData.Slice()
                        {
                            Data = levelData,
                            Height = levels[i].Height,
                            Width = levels[i].Width,
                            Pitch = pitch,
                            TotalBytes = levelData.Length,
                        };
                    }
                }

                data.Format = gFormat;
                data.IsCompressed = true;
            }
        }

        private static byte[] CompressLevel(BCBlockParser parser, TextureData.Slice uncompressed, Logger log)
        {
            int blockCountX = Math.Max(1, (uncompressed.Width + 3) / BLOCK_DIMENSIONS);
            int blockCountY = Math.Max(1, (uncompressed.Height + 3) / BLOCK_DIMENSIONS);
            int blockWidth = Math.Min(uncompressed.Width, BLOCK_DIMENSIONS);
            int blockHeight = Math.Min(uncompressed.Height, BLOCK_DIMENSIONS);
            byte[] result = null;
            Stopwatch blockTimer = new Stopwatch();
            Stopwatch mainTimer = new Stopwatch();

            mainTimer.Start();
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        blockTimer.Reset();
                        blockTimer.Start();
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {

                            // Assemble color table for current block.
                            int index = 0;
                            Color4[] colTable = new Color4[BC.NUM_PIXELS_PER_BLOCK];

                            for (int bpy = 0; bpy < BLOCK_DIMENSIONS; bpy++)
                            {
                                int py = (blockY << 2) + bpy;
                                for (int bpx = 0; bpx < BLOCK_DIMENSIONS; bpx++)
                                {

                                    int px = (blockX << 2) + bpx;
                                    if ((px < uncompressed.Width) && (py < uncompressed.Height))
                                    {
                                        int offset = ((py * uncompressed.Width) + px) << 2;
                                        colTable[index++] = new Color()
                                        {
                                            R = uncompressed.Data[offset],
                                            G = uncompressed.Data[offset + 1],
                                            B = uncompressed.Data[offset + 2],
                                            A = uncompressed.Data[offset + 3]
                                        };
                                    }
                                }
                            }

                            parser.Encode(writer, colTable, log);
                        }
                        blockTimer.Stop();
                        log.WriteLine($"Encoded block row ${blockY} in {blockTimer.Elapsed.TotalMilliseconds.ToString("N2")}ms");
                    }

                    result = stream.ToArray();
                }
            }
            mainTimer.Stop();
            log.WriteLine($"Encoded BC6H {uncompressed.Width}x{uncompressed.Height} mip-map level in ${mainTimer.Elapsed.TotalMilliseconds.ToString("N2")}ms");

            return result;
        }

        /// <summary>Returns the expected block size of the provided format. Only block-compressed formats will return a valid value. <para/>
        /// A block is 4x4 pixels.</summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static int GetBlockSize(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.BC1_UNorm:
                case GraphicsFormat.BC1_UNorm_SRgb:
                case GraphicsFormat.BC4_SNorm:
                case GraphicsFormat.BC4_UNorm:
                case GraphicsFormat.BC4_Typeless:
                    return 8;

                case GraphicsFormat.BC2_UNorm:
                case GraphicsFormat.BC2_UNorm_SRgb:
                case GraphicsFormat.BC3_UNorm:
                case GraphicsFormat.BC3_UNorm_SRgb:
                case GraphicsFormat.BC5_SNorm:
                case GraphicsFormat.BC5_UNorm:
                case GraphicsFormat.BC5_Typeless:
                case GraphicsFormat.BC6H_Uf16:
                case GraphicsFormat.BC6H_Sf16:
                case GraphicsFormat.BC6H_Typeless:
                case GraphicsFormat.BC7_UNorm_SRgb:
                case GraphicsFormat.BC7_UNorm:
                case GraphicsFormat.BC7_Typeless:
                    return 16;
            }

            return 0;
        }

        /// <summary>
        /// Returns true if the specified format is a block-compressed format.
        /// </summary>
        /// <param name="format">The graphics format.</param>
        /// <returns>A boolean value.</returns>
        public static bool GetBlockCompressed(GraphicsFormat format)
        {
            return GetBlockSize(format) > 0;
        }

        /// <summary>Gets the expected number of bytes for a slice matching the provided size and format.</summary>
        /// <param name="format">The format to use in the calculation.</param>
        /// <param name="height">The expected height.</param>
        /// <param name="width">The expected width.</param>
        /// <returns></returns>
        public static int GetBCSliceSize(GraphicsFormat format, int width, int height)
        {
            int blockSize = GetBlockSize(format);
            int blockCountX = Math.Max(1,(width + 3) / 4);
            int blockCountY = Math.Max(1,(height + 3) / 4);

            return (blockCountX * blockSize) * blockCountY;
        }

        /// <summary>Gets the block-compressed pitch size of a mip-map level</summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="blockSize">The number of bytes per block.</param>
        /// <returns></returns>
        public static int GetBCPitch(int width, int height, int blockSize)
        {
            int numBlocksWide = Math.Max(1, (width + 3) / 4);
            return numBlocksWide * blockSize;
        }

        /// <summary>Gets the block-compressed size of a mip-map level, in bytes.</summary>
        /// <param name="width">The width of the level.</param>
        /// <param name="height">The height of the level.</param>
        /// <param name="blockSize">The block size of the compression format.</param>
        /// <returns></returns>
        public static int GetBCLevelSize(int width, int height, int blockSize)
        {
            int numBlocksWide = Math.Max(1, (width + 3) / 4);
            int numBlocksHigh = Math.Max(1, (height + 3) / 4);
            int blockPitch = numBlocksWide * blockSize;
            return blockPitch * numBlocksHigh;
        }

        public static void GetBCLevelSizeAndPitch(int width, int height, int blockSize, out int levelSize, out int blockPitch)
        {
            int numBlocksWide = Math.Max(1, (width + 3) / 4);
            int numBlocksHigh = Math.Max(1, (height + 3) / 4);
            blockPitch = numBlocksWide * blockSize;
            levelSize = blockPitch * numBlocksHigh;
        }
    }
}
