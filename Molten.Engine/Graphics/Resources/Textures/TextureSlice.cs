﻿using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
    public unsafe class TextureSlice : IDisposable
    {
        byte* _data;

        public byte* Data => _data;

        public uint Pitch;
        public uint TotalBytes { get; private set; }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        List<TextureSliceRef> _references = new List<TextureSliceRef>();

        public TextureSlice(uint width, uint height, uint numBytes)
        {
            Width = width;
            Height = height;
            Allocate(numBytes);
        }

        public TextureSlice(uint width, uint height, byte* data, uint numBytes)
        {
            Width = width;
            Height = height;
            _data = data;
            TotalBytes = numBytes;
        }

        public TextureSlice(uint width, uint height, byte[] data, uint startIndex, uint numBytes)
        {
            Width = width;
            Height = height;
            Allocate(numBytes);

            fixed (byte* ptrData = data)
            {
                byte* ptr = ptrData + startIndex;
                Buffer.MemoryCopy(ptr, Data, numBytes, numBytes);
            }
        }

        public TextureSlice(uint width, uint height, byte[] data)
        {
            Width = width;
            Height = height;
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
        internal static unsafe TextureSlice FromTextureSlice(GraphicsQueue cmd, GraphicsTexture tex, GraphicsTexture staging, uint level, uint arraySlice, GraphicsMapType mapType)
        {
            uint subID = (arraySlice * tex.MipMapCount) + level;
            uint subWidth = tex.Width >> (int)level;
            uint subHeight = tex.Height >> (int)level;

            GraphicsResource resMap = tex as GraphicsResource;
            GraphicsResource resStaging = staging as GraphicsResource;

            if (staging != null)
            {
                cmd.CopyResourceRegion(resMap, subID, null, resStaging, subID, Vector3UI.Zero);
                cmd.Profiler.Current.CopySubresourceCount++;
                resMap = resStaging;
            }

            uint blockSize = BCHelper.GetBlockSize(tex.ResourceFormat);
            uint expectedRowPitch = 4 * tex.Width; // 4-bytes per pixel * Width.
            uint expectedSlicePitch = expectedRowPitch * tex.Height;

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
                    uint p = 0;
                    while (p < stream.Map.DepthPitch)
                    {
                        stream.ReadRange(ptrSlice, expectedRowPitch);
                        ptrSlice += expectedRowPitch;
                        p += stream.Map.RowPitch;
                    }
                }
            }

            TextureSlice slice = new TextureSlice(subWidth, subHeight, sliceData)
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
            TextureSlice result = new TextureSlice(Width, Height, TotalBytes)
            {
                Pitch = Pitch,
                TotalBytes = TotalBytes,
            };

            Buffer.MemoryCopy(_data, result._data, TotalBytes, TotalBytes);

            return result;
        }
    }
}
