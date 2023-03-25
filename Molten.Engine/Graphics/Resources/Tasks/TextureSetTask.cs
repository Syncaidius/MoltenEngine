using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public unsafe struct TextureSetTask<T> : IGraphicsResourceTask
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

        public Action<GraphicsResource> CompleteCallback;

        public TextureSetTask(T* data, uint startIndex, uint numElements)
        {
            Stride = (uint)sizeof(T);
            NumElements = numElements;
            NumBytes = Stride * NumElements;

            _data = (T*)EngineUtil.Alloc(NumBytes);

            T* ptrStart = data + startIndex;
            Buffer.MemoryCopy(ptrStart, Data, NumBytes, NumBytes);
        }

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            ITexture2D texture = resource as ITexture2D;

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

            if (texture.Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                using (GraphicsStream stream = cmd.MapResource(resource, subLevel, 0))
                {
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
                        stream.WriteRange(ptrData, NumBytes);
                    }
                }
                cmd.Profiler.Current.MapDiscardCount++;
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
                    cmd.UpdateResource(resource, subLevel, null, ptrData, bcPitch, arraySliceBytes);
                }
                else
                {
                    if (Area != null)
                    {
                        RectangleUI rect = Area.Value;
                        uint areaPitch = Stride * rect.Width;
                        ResourceRegion region = new ResourceRegion(rect.X, rect.Y, 0, rect.Right, rect.Bottom, 1);
                        cmd.UpdateResource(resource, subLevel, region, ptrData, areaPitch, NumBytes);
                    }
                    else
                    {
                        cmd.UpdateResource(resource, subLevel, null, ptrData, Pitch, arraySliceBytes);
                    }
                }
            }

            EngineUtil.Free(ref _data);
            CompleteCallback?.Invoke(resource);
            return true;
        }
    }
}
