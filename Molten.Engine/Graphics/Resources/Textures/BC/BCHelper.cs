using System.Diagnostics;
using System.Reflection;

namespace Molten.Graphics.Textures;

/// <summary>
/// Texture block-compression helper class.
/// </summary>
internal static class BCHelper
{
    /// <summary>
    /// The expected width and height of a DDS data block, in pixels.
    /// </summary>
    public const int BLOCK_DIMENSIONS = 4;

    static Dictionary<GraphicsFormat, BCBlockParser> _parsers;

    static BCHelper()
    {
        _parsers = new Dictionary<GraphicsFormat, BCBlockParser>();
        IEnumerable<Type> parserTypes = ReflectionHelper.FindType<BCBlockParser>();
        foreach (Type t in parserTypes)
        {
            BCBlockParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as BCBlockParser;
            _parsers.Add(parser.ExpectedFormat, parser);
        }
    }

    internal unsafe static void Decompress(TextureData data, Logger log)
    {
        // Cannot decompress uncompressed data.
        if (data.IsCompressed == false)
            return;

        TextureSlice[] levels = data.Levels;
        data.Levels = new TextureSlice[levels.Length];

        BCBlockParser parser = null;

        if (_parsers.TryGetValue(data.Format, out parser))
        {
            for (uint a = 0; a < data.ArraySize; a++)
            {
                for (uint i = 0; i < data.MipMapLevels; i++)
                {
                    uint levelID = (a * data.MipMapLevels) + i;
                    byte[] decompressed = DecompressLevel(parser, levels[levelID], log);

                    data.Levels[levelID] = new TextureSlice(levels[i].Width, levels[i].Height, 1, decompressed)
                    {
                        Pitch = levels[i].Width * 4,
                    };
                }
            }

            data.Format = GraphicsFormat.R8G8B8A8_UNorm;
            data.IsCompressed = false;
        }
    }

    private unsafe static byte[] DecompressLevel(BCBlockParser parser, TextureSlice compressed, Logger log)
    {
        // Pass to stream-based overload
        byte[] result = new byte[compressed.Width * compressed.Height * 4];

        using (UnmanagedMemoryStream stream = new UnmanagedMemoryStream(compressed.Data, compressed.TotalBytes))
        {
            using (BinaryReader imageReader = new BinaryReader(stream))
            {
                uint blockCountX = Math.Max(1, (compressed.Width + 3) / BLOCK_DIMENSIONS);
                uint blockCountY = Math.Max(1, (compressed.Height + 3) / BLOCK_DIMENSIONS);
                uint blockWidth = Math.Min(compressed.Width, BLOCK_DIMENSIONS);
                uint blockHeight = Math.Min(compressed.Height, BLOCK_DIMENSIONS);

                for (uint blockY = 0; blockY < blockCountY; blockY++)
                {
                    for (uint blockX = 0; blockX < blockCountX; blockX++)
                    {
                        Color4[] pixels = parser.Decode(imageReader, log);

                        // Transfer the decompressed pixel data into the image.
                        uint index = 0;
                        for (uint bpy = 0; bpy < BLOCK_DIMENSIONS; bpy++)
                        {
                            uint py = (blockY << 2) + bpy;
                            for (uint bpx = 0; bpx < BLOCK_DIMENSIONS; bpx++)
                            {
                                Color c = (Color)pixels[index++];

                                uint px = (blockX << 2) + bpx;
                                if ((px < compressed.Width) && (py < compressed.Height))
                                {
                                    uint offset = ((py * compressed.Width) + px) << 2;
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
    public unsafe static void Compress(TextureData data, DDSFormat compressionFormat, Logger log)
    {
        // Cannot compress data which is already compressed.
        if (data.IsCompressed)
            return;

        if (data.Width % 4 > 0 || data.Height % 4 > 0)
            throw new DDSSizeException(compressionFormat, data.Width, data.Height);

        TextureSlice[] levels = data.Levels;
        data.Levels = new TextureSlice[levels.Length];
        GraphicsFormat gFormat = compressionFormat.ToGraphicsFormat();

        BCBlockParser parser = null;
        if (_parsers.TryGetValue(gFormat, out parser))
        {
            for (uint a = 0; a < data.ArraySize; a++)
            {
                for (uint i = 0; i < data.MipMapLevels; i++)
                {
                    uint levelID = (a * data.MipMapLevels) + i;
                    byte[] levelData = CompressLevel(parser, levels[levelID], log);
                    uint pitch = Math.Max(1, ((levels[i].Width + 3) / 4) * GetBlockSize(gFormat));

                    data.Levels[levelID] = new TextureSlice(levels[i].Width, levels[i].Height, 1, levelData)
                    {
                        Pitch = pitch,
                    };
                }
            }

            data.Format = gFormat;
            data.IsCompressed = true;
        }
    }

    private unsafe static byte[] CompressLevel(BCBlockParser parser, TextureSlice uncompressed, Logger log)
    {
        uint blockCountX = Math.Max(1, (uncompressed.Width + 3) / BLOCK_DIMENSIONS);
        uint blockCountY = Math.Max(1, (uncompressed.Height + 3) / BLOCK_DIMENSIONS);
        byte[] result = null;
        Stopwatch mainTimer = new Stopwatch();

        mainTimer.Start();
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (uint blockY = 0; blockY < blockCountY; blockY++)
                {
                    for (uint blockX = 0; blockX < blockCountX; blockX++)
                    {

                        // Assemble color table for current block.
                        uint index = 0;
                        Color4[] colTable = new Color4[BC.NUM_PIXELS_PER_BLOCK];

                        for (uint bpy = 0; bpy < BLOCK_DIMENSIONS; bpy++)
                        {
                            uint py = (blockY << 2) + bpy;
                            for (uint bpx = 0; bpx < BLOCK_DIMENSIONS; bpx++)
                            {

                                uint px = (blockX << 2) + bpx;
                                if ((px < uncompressed.Width) && (py < uncompressed.Height))
                                {
                                    uint offset = ((py * uncompressed.Width) + px) << 2;
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
                }

                result = stream.ToArray();
            }
        }
        mainTimer.Stop();
        log.WriteLine($"Encoded {parser.ExpectedFormat} {uncompressed.Width}x{uncompressed.Height} mip-map level in {mainTimer.Elapsed.TotalMilliseconds.ToString("N2")}ms");

        return result;
    }

    /// <summary>Returns the expected block size of the provided format. Only block-compressed formats will return a valid value. <para/>
    /// A block is 4x4 pixels.</summary>
    /// <param name="format">The format.</param>
    /// <returns></returns>
    public static uint GetBlockSize(GraphicsFormat format)
    {
        switch (format)
        {
            case GraphicsFormat.BC1_UNorm:
            case GraphicsFormat.BC1_UNorm_SRgb:
            case GraphicsFormat.BC4_SNorm:
            case GraphicsFormat.BC4_UNorm:
            case GraphicsFormat.BC4_Typeless:
                return 8U;

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
                return 16U;
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
    public static uint GetBCSliceSize(GraphicsFormat format, uint width, uint height)
    {
        uint blockSize = GetBlockSize(format);
        uint blockCountX = Math.Max(1, (width + 3) / 4);
        uint blockCountY = Math.Max(1, (height + 3) / 4);

        return (blockCountX * blockSize) * blockCountY;
    }

    /// <summary>
    /// Gets the total size of a texture of the given dimensions.
    /// </summary>        
    /// <param name="format">The format to use in the calculation.</param>
    /// <param name="height">The expected height.</param>
    /// <param name="width">The expected width.</param>
    /// <param name="numLevels">The number of mip-map levels.</param>
    public static uint GetBCSize(GraphicsFormat format, uint width, uint height, uint numLevels)
    {
        uint totalBytes = 0;
        for(uint i = 0; i < numLevels; i++)
        {
            totalBytes += GetBCSliceSize(format, width, height);
            width /= 2;
            height /= 2;
        }

        return totalBytes;
    }

    /// <summary>Gets the block-compressed pitch size of a mip-map level</summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="blockSize">The number of bytes per block.</param>
    /// <returns></returns>
    public static uint GetBCPitch(uint width, uint blockSize)
    {
        uint numBlocksWide = Math.Max(1, (width + 3) / 4);
        return numBlocksWide * blockSize;
    }

    /// <summary>Gets the block-compressed size of a mip-map level, in bytes.</summary>
    /// <param name="width">The width of the level.</param>
    /// <param name="height">The height of the level.</param>
    /// <param name="blockSize">The block size of the compression format.</param>
    /// <returns></returns>
    public static uint GetBCLevelSize(uint width, uint height, uint blockSize)
    {
        uint numBlocksWide = Math.Max(1, (width + 3) / 4);
        uint numBlocksHigh = Math.Max(1, (height + 3) / 4);
        uint blockPitch = numBlocksWide * blockSize;
        return blockPitch * numBlocksHigh;
    }

    public static void GetBCLevelSizeAndPitch(uint width, uint height, uint blockSize, out uint levelSize, out uint blockPitch)
    {
        uint numBlocksWide = Math.Max(1, (width + 3) / 4);
        uint numBlocksHigh = Math.Max(1, (height + 3) / 4);
        blockPitch = numBlocksWide * blockSize;
        levelSize = blockPitch * numBlocksHigh;
    }
}
