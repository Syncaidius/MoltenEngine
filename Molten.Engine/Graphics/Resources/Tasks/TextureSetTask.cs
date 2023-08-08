using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public unsafe struct TextureSetTask : IGraphicsResourceTask
    {
        public uint MipLevel;

        public void* Data;

        public uint StartIndex;

        public uint Pitch;

        public uint ArrayIndex;

        public GraphicsMapType MapType;

        public uint NumElements { get; private set; }

        public uint NumBytes { get; private set; }

        public uint Stride { get; private set; }

        public ResourceRegion? Area;

        public Action<GraphicsResource> CompleteCallback;

        public TextureSetTask(void* data, uint stride, uint startIndex, uint numElements)
        {
            Stride = stride;
            NumElements = numElements;
            NumBytes = Stride * NumElements;
            Data = EngineUtil.Alloc(NumBytes);

            void* ptrStart = (byte*)data + startIndex;
            Buffer.MemoryCopy(ptrStart, Data, NumBytes, NumBytes);
        }

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            GraphicsTexture texture = resource as GraphicsTexture;

            // Calculate size of a single array slice
            uint arraySliceBytes = 0;
            uint blockSize = 8; // default block size
            uint levelWidth = texture.Width;
            uint levelHeight = texture.Height;
            uint levelDepth = texture.Depth;

            if (texture.IsBlockCompressed)
            {
                if (Area != null)
                    throw new NotImplementedException("Area-based SetData on block-compressed texture is currently unsupported. Sorry!");

                blockSize = BCHelper.GetBlockSize(texture.ResourceFormat);

                // Collect total level size.
                for (uint i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += BCHelper.GetBCLevelSize(levelWidth, levelHeight, blockSize) * levelDepth;
                    levelWidth = Math.Max(1, levelWidth / 2);
                    levelHeight = Math.Max(1, levelHeight / 2);
                    levelDepth = Math.Max(1, levelDepth / 2);
                }
            }
            else
            {
                // TODO: This is invalid if the format isn't 32bpp/4-bytes-per-pixel/RGBA.
                for (uint i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += (levelWidth * levelHeight * 4) * levelDepth; //4 color channels. 1 byte each. Width * height * colorByteSize.
                    levelWidth = Math.Max(1, levelWidth / 2);
                    levelHeight = Math.Max(1, levelHeight / 2);
                    levelDepth = Math.Max(1, levelDepth / 2);
                }
            }

            //======DATA TRANSFER===========
            uint startBytes = StartIndex * Stride;
            byte* ptrData = (byte*)Data;
            ptrData += startBytes;

            uint subLevel = (texture.MipMapCount * ArrayIndex) + MipLevel;

            if (texture.Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                using (GraphicsStream stream = cmd.MapResource(resource, subLevel, 0, MapType))
                {
                    // Are we constrained to an area of the texture?
                    if (Area != null)
                    {
                        ResourceRegion area = Area.Value;
                        uint areaPitch = Stride * area.Width;
                        uint sliceBytes = areaPitch * area.Height;
                        uint aX = area.Left;
                        uint aY = area.Top;
                        uint aZ = area.Front;

                        for (uint y = aY, end = area.Bottom; y < end; y++)
                        {
                            stream.Position = (sliceBytes * aZ) + (Pitch * aY) + (aX * Stride);
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
                cmd.Profiler.ResourceMapCalls++;
            }
            else
            {
                if (texture.IsBlockCompressed)
                {
                    // Calculate mip-map level size.
                    levelWidth = Math.Max(1, texture.Width >> (int)MipLevel);
                    uint bcPitch = BCHelper.GetBCPitch(levelWidth, blockSize);

                    // TODO support copy flags (DX11.1 feature)
                    cmd.UpdateResource(resource, subLevel, null, ptrData, bcPitch, arraySliceBytes);
                }
                else
                {
                    if (Area != null)
                    {
                        uint rowPitch = Stride * Area.Value.Width;
                        cmd.UpdateResource(resource, subLevel, Area.Value, ptrData, rowPitch, NumBytes);
                    }
                    else
                    {
                        cmd.UpdateResource(resource, subLevel, null, ptrData, Pitch, arraySliceBytes);
                    }
                }
            }

            EngineUtil.Free(ref Data);
            CompleteCallback?.Invoke(resource);
            return true;
        }
    }
}
