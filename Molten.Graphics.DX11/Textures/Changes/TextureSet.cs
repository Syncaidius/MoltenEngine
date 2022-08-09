using Molten.Graphics.Textures;
using Molten.IO;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class TextureSet<T> : ITextureTask, IDisposable
        where T: unmanaged
    {
        T* _data;

        public uint MipLevel;

        public T* Data => _data;

        public uint StartIndex;
        public uint Pitch;
        public uint ArrayIndex;

        public uint NumElements { get; private set; }

        public uint NumBytes { get; private set; }

        public uint Stride { get; private set; }

        public RectangleUI? Area;

        public bool UpdatesTexture => true;

        public TextureSet(T[] data, uint startIndex, uint numElements)
        {
            Stride = (uint)sizeof(T);
            NumElements = numElements;
            NumBytes = Stride * NumElements;

            _data = (T*)EngineUtil.Alloc(NumBytes);

            fixed (T* ptrData = data)
            {
                T* ptrStart = ptrData + startIndex;
                Buffer.MemoryCopy(ptrStart, Data, NumBytes, NumBytes);
            }
        }

        public TextureSet(T* data, uint startIndex, uint numElements)
        {
            Stride = (uint)sizeof(T);
            NumElements = numElements;
            NumBytes = Stride * NumElements;

            _data = (T*)EngineUtil.Alloc(NumBytes);

            T* ptrStart = data + startIndex;
            Buffer.MemoryCopy(ptrStart, Data, NumBytes, NumBytes);
        }

        ~TextureSet()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_data != null)
                EngineUtil.Free(ref _data);
        }

        public unsafe bool Process(DeviceContext context, TextureBase texture)
        {
            // Calculate size of a single array slice
            uint arraySliceBytes = 0;
            uint blockSize = 8; // default block size
            uint levelWidth = texture.Width;
            uint levelHeight = texture.Height;

            if (texture.IsBlockCompressed)
            {
                if (Area != null)
                    throw new NotImplementedException("Area-based SetData on block-compressed texture is currently unsupported. Sorry!");

                blockSize = BCHelper.GetBlockSize(texture.DataFormat);

                // Collect total level size.
                for (uint i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += BCHelper.GetBCLevelSize(levelWidth, levelHeight, blockSize);
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }
            else
            {
                // TODO: This is invalid if the format isn't 32bpp/4-bytes-per-pixel/RGBA.
                for (uint i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += levelWidth * levelHeight * 4; //4 color channels. 1 byte each. Width * height * colorByteSize.
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            //======DATA TRANSFER===========
            uint startBytes = StartIndex * Stride;
            byte* ptrData = (byte*)Data;
            ptrData += startBytes;

            uint subLevel = (texture.MipMapCount * ArrayIndex) + MipLevel;

            if (texture.HasFlags(TextureFlags.Dynamic))
            {
                RawStream stream = null;

                MappedSubresource destBox = context.MapResource(
                    texture.NativePtr,
                    subLevel,
                    Map.WriteDiscard,
                    0,
                    out stream);

                // Are we constrained to an area of the texture?
                if (Area != null)
                {
                    RectangleUI rect = Area.Value;
                    uint areaPitch = Stride * rect.Width;
                    uint aX = rect.X;
                    uint aY = rect.Y;

                    for (uint y = aY, end = rect.Bottom; y < end; y++)
                    {
                        stream.Position = (Pitch * aY) + (aX * Stride);
                        stream.WriteRange(ptrData, areaPitch);
                        ptrData += areaPitch;
                        aY++;
                    }
                }
                else
                {
                    long numBytes = NumElements * Stride;
                    stream.WriteRange(ptrData, NumElements);
                }

                context.UnmapResource(texture.NativePtr, subLevel);
                context.Profiler.Current.MapDiscardCount++;
            }
            else
            {
                if (texture.IsBlockCompressed)
                {
                    // Calculate mip-map level size.
                    levelWidth = texture.Width >> (int)MipLevel;
                    levelHeight = texture.Height >> (int)MipLevel;
                    uint bcPitch = BCHelper.GetBCPitch(levelWidth, levelHeight, blockSize);

                    // TODO support copy flags (DX11.1 feature)
                    context.UpdateResource(texture, subLevel, null, ptrData, bcPitch, arraySliceBytes);
                }
                else
                {
                    if (Area != null)
                    {
                        RectangleUI rect = Area.Value;
                        uint areaPitch = Stride * rect.Width;
                        Box region = new Box();
                        region.Top = rect.Y;
                        region.Front = 0;
                        region.Back = 1;
                        region.Bottom = rect.Bottom;
                        region.Left = rect.X;
                        region.Right = rect.Right;

                        uint numBytes = NumElements * Stride;
                        context.UpdateResource(texture, subLevel, &region, ptrData, areaPitch, numBytes);
                    }
                    else
                    {
                        //uint x = 0;
                        //uint y = 0;
                        //uint w = Math.Max(texture.Width >> (int)MipLevel, 1);
                        //uint h = Math.Max(texture.Height >> (int)MipLevel, 1);
                        context.UpdateResource(texture, subLevel, null, ptrData, Pitch, arraySliceBytes);
                    }
                }
            }

            return true;
        }
    }
}
