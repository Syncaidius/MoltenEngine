using Molten.Graphics.Textures;

namespace Molten.Graphics;

/// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
public unsafe class TextureSlice : IDisposable
{
    byte* _data;

    /// <summary>
    /// Gets the data pointer of the texture slice.
    /// </summary>
    public byte* Data => _data;

    /// <summary>
    /// Gets the row pitch of the texture slice. This is the number of bytes in a single row of pixels/blocks.
    /// </summary>
    public uint Pitch;

    /// <summary>
    /// Gets the total number of bytes in the texture slice.
    /// </summary>
    public uint TotalBytes { get; private set; }

    /// <summary>
    /// Gets the width of the texture slice, in pixels.
    /// </summary>
    /// 
    public uint Width { get; private set; }

    /// <summary>
    /// Gets the height of the texture slice, in pixels.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// Gets the depth of the texture slice. This is only relevant for 3D textures.
    /// </summary>
    public uint Depth { get; private set; }

    List<TextureSliceRef> _references = new List<TextureSliceRef>();

    /// <summary>
    /// Create a new instance of <see cref="TextureSlice"/>.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="numBytes"></param>
    public TextureSlice(uint width, uint height, uint depth, uint numBytes)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Allocate(numBytes);
    }

    /// <summary>
    /// Create a new instance of <see cref="TextureSlice"/>.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="data"></param>
    /// <param name="numBytes"></param>
    public TextureSlice(uint width, uint height, uint depth, byte* data, uint numBytes)
    {
        Width = width;
        Height = height;
        Depth = depth;
        _data = data;
        TotalBytes = numBytes;
    }

    /// <summary>
    /// Create a new instance of <see cref="TextureSlice"/>.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="data"></param>
    /// <param name="startIndex"></param>
    /// <param name="numBytes"></param>
    public TextureSlice(uint width, uint height, uint depth, byte[] data, uint startIndex, uint numBytes)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Allocate(numBytes);

        fixed (byte* ptrData = data)
        {
            byte* ptr = ptrData + startIndex;
            Buffer.MemoryCopy(ptr, Data, numBytes, numBytes);
        }
    }

    /// <summary>
    /// Create a new instance of <see cref="TextureSlice"/>.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="data"></param>
    public TextureSlice(uint width, uint height, uint depth, byte[] data)
    {
        Width = width;
        Height = height;
        Depth = depth;
        uint numBytes = (uint)data.Length;

        Allocate(numBytes);

        fixed (byte* ptrData = data)
            Buffer.MemoryCopy(ptrData, Data, numBytes, numBytes);
    }

    ~TextureSlice()
    {
        Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The reference data type.</typeparam>
    /// <returns></returns>
    public TextureSliceRef<T> GetReference<T>()
        where T : unmanaged
    {
        TextureSliceRef<T> sr = new TextureSliceRef<T>(this);
        _references.Add(sr);
        return sr;
    }

    public void Allocate(uint numBytes)
    {
        if (Data != null)
            EngineUtil.Free(ref _data);

        TotalBytes = numBytes;
        _data = (byte*)EngineUtil.Alloc(numBytes);
        foreach (TextureSliceRef sr in _references)
            sr.UpdateReference();
    }

    /// <summary>Gets a new instance of <see cref="TextureSlice"/> that is populated with data from a texture <see cref="GraphicsResource"/>.</summary>
    /// <param name="cmd">The command queue that is to perform the retrieval.</param>
    /// <param name="staging">The staging texture to copy the data to.</param>
    /// <param name="level">The mip-map level.</param>
    /// <param name="arraySlice">The array slice.</param>
    /// <returns></returns>
    internal static unsafe TextureSlice FromTextureSlice(GraphicsQueue cmd, GraphicsTexture tex, uint level, uint arraySlice, GraphicsMapType mapType)
    {
        uint subID = (arraySlice * tex.MipMapCount) + level;
        uint subWidth = tex.Width >> (int)level;
        uint subHeight = tex.Height >> (int)level;
        uint subDepth = tex.Depth >> (int)level;

        GraphicsResource resMap = tex;

        uint blockSize = BCHelper.GetBlockSize(tex.ResourceFormat);
        uint expectedRowPitch = 4 * tex.Width; // 4-bytes per pixel * Width.
        uint expectedSlicePitch = (expectedRowPitch * tex.Height) * tex.Depth;

        if (blockSize > 0)
            BCHelper.GetBCLevelSizeAndPitch(subWidth, subHeight, blockSize, out expectedSlicePitch, out expectedRowPitch);

        byte[] sliceData = new byte[expectedSlicePitch];

        // Now pull data from it
        using (GraphicsStream stream = cmd.MapResource(resMap, subID, 0, mapType))
        {
            // NOTE: Databox: "The row pitch in the mapping indicate the offsets you need to use to jump between rows."
            // https://gamedev.stackexchange.com/questions/106308/problem-with-id3d11devicecontextcopyresource-method-how-to-properly-read-a-t/106347#106347

            fixed (byte* ptrFixedSlice = sliceData)
            {
                byte* ptrSlice = ptrFixedSlice;
                ulong p = 0;
                while (p < stream.Map.DepthPitch)
                {
                    stream.ReadRange(ptrSlice, expectedRowPitch);
                    ptrSlice += expectedRowPitch;
                    p += stream.Map.RowPitch;
                }
            }
        }

        TextureSlice slice = new TextureSlice(subWidth, subHeight, subDepth, sliceData)
        {
            Pitch = expectedRowPitch,
        };

        return slice;
    }

    public void Dispose()
    {
        if (Data != null)
            EngineUtil.Free(ref _data);
    }

    public TextureSlice Clone()
    {
        TextureSlice result = new TextureSlice(Width, Height, Depth, TotalBytes)
        {
            Pitch = Pitch,
            TotalBytes = TotalBytes,
        };

        Buffer.MemoryCopy(_data, result._data, TotalBytes, TotalBytes);

        return result;
    }
}
