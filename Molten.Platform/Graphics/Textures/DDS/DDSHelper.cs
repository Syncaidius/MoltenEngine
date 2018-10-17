using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    public static class DDSHelper
    {
        static Dictionary<GraphicsFormat, BCBlockParser> _parsers;

        static DDSHelper()
        {
            _parsers = new Dictionary<GraphicsFormat, BCBlockParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindType<BCBlockParser>();
            foreach(Type t in parserTypes)
            {
                BCBlockParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as BCBlockParser;
                foreach (GraphicsFormat supportedFormat in parser.SupportedFormats)
                    _parsers.Add(supportedFormat, parser);
            }
        }

        public static void Decompress(TextureData data)
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
                        byte[] levelData = parser.Decompress(levels[levelID]);

                        data.Levels[levelID] = new TextureData.Slice()
                        {
                            Data = levelData,
                            Height = levels[i].Height,
                            Width = levels[i].Width,
                            Pitch = levels[i].Width * 4,
                            TotalBytes = levelData.Length,
                        };
                    }
                }

                data.Format = GraphicsFormat.R8G8B8A8_UNorm;
                data.IsCompressed = false;
            }
        }

        /// <summary>Compresses the texture data into DXT5 block-compressed format, if not already compressed.</summary>
        /// <param name="data"></param>
        public static void Compress(TextureData data, DDSFormat compressionFormat)
        {
            // Cannot compress data which is already compressed.
            if (data.IsCompressed)
                return;

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
                        byte[] levelData = parser.Compress(levels[levelID]);
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

        /// <summary>Generates garbage data and fills the texture with it.</summary>
        /// <returns></returns>
        public static TextureData GenerateFakeData(TextureData source)
        {
            // TODO use DDSFakeParser.
            return null;
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
        public static int GetSliceSize(GraphicsFormat format, int width, int height)
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
            int blockCountX = (width + 3) / 4;
            int numBlocksWide = Math.Max(1, blockCountX);
            int blockPitch = numBlocksWide * blockSize;

            return blockPitch;
        }

        /// <summary>Gets the block-compressed size of a mip-map level, in bytes.</summary>
        /// <param name="width">The width of the level.</param>
        /// <param name="height">The height of the level.</param>
        /// <param name="blockSize">The block size of the compression format.</param>
        /// <returns></returns>
        public static int GetBCLevelSize(int width, int height, int blockSize)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;

            int numBlocksWide = Math.Max(1, blockCountX);
            int numBlocksHigh = Math.Max(1, blockCountY);
            int numRows = numBlocksHigh;

            int blockPitch = numBlocksWide * blockSize;

            int pitch = width * 4;
            return blockPitch * numRows;
        }
    }
}
