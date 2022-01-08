using Molten.Graphics.Textures;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class TextureSet<T> : ITextureChange where T: struct
    {
        public int MipLevel;
        public T[] Data;
        public int StartIndex;
        public int Pitch;
        public int ArrayIndex;

        public int Count;
        public int Stride;
        public Rectangle? Area;

        public unsafe void Process(PipeDX11 pipe, TextureBase texture)
        {
            //C alculate size of a single array slice
            int arraySliceBytes = 0;
            int blockSize = 8; // default block size
            int levelWidth = texture.Width;
            int levelHeight = texture.Height;

            if (texture.IsBlockCompressed)
            {
                if (Area != null)
                    throw new NotImplementedException("Area-based SetData on block-compressed texture is currently unsupported. Sorry!");

                blockSize = BCHelper.GetBlockSize(texture.DataFormat);

                // Collect total level size.
                for (int i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += BCHelper.GetBCLevelSize(levelWidth, levelHeight, blockSize);
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }
            else
            {
                // TODO: This is invalid if the format isn't 32bpp/4-bytes-per-pixel/RGBA.
                for (int i = 0; i < texture.MipMapCount; i++)
                {
                    arraySliceBytes += levelWidth * levelHeight * 4; //4 color channels. 1 byte each. Width * height * colorByteSize.
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            //======DATA TRANSFER===========
            EngineInterop.PinObject(Data, (ptr) =>
            {
                int startBytes = StartIndex * Stride;
                byte* ptrData = (byte*)ptr.ToPointer();
                ptrData += startBytes;

                int subLevel = ((int)texture.MipMapCount * ArrayIndex) + MipLevel;

                if (texture.HasFlags(TextureFlags.Dynamic))
                {
                    ResourceStream stream = null;

                    MappedSubresource destBox = pipe.MapResource(
                        texture.NativePtr, 
                        (uint)subLevel, 
                        Map.MapWriteDiscard, 
                        0, 
                        out stream);

                    // Are we constrained to an area of the texture?
                    if (Area != null)
                    {
                        Rectangle rect = Area.Value;
                        int areaPitch = Stride * rect.Width;
                        int aX = rect.X;
                        int aY = rect.Y;

                        for (int y = aY, end = rect.Bottom; y < end; y++)
                        {
                            stream.Position = (Pitch * aY) + (aX * Stride);
                            stream.WriteRange(ptrData, areaPitch);
                            ptrData += areaPitch;
                            aY++;
                        }
                    }
                    else
                    {
                        long numBytes = Count * Stride;
                        stream.WriteRange(ptrData, Count);
                    }

                    pipe.UnmapResource(texture.NativePtr, (uint)subLevel);
                    pipe.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    if (texture.IsBlockCompressed)
                    {
                        // Calculate mip-map level size.
                        levelWidth = (int)texture.Width >> (int)MipLevel;
                        levelHeight = (int)texture.Height >> (int)MipLevel;
                        int bcPitch = BCHelper.GetBCPitch(levelWidth, levelHeight, blockSize);
                        DataBox box = new DataBox(dataPtr, bcPitch, arraySliceBytes);
                        pipe.Context.UpdateSubresource1(box, texture.UnderlyingResource, subLevel);
                    }
                    else
                    {
                        if (Area != null)
                        {
                            Rectangle rect = Area.Value;
                            int areaPitch = Stride * rect.Width;
                            DataBox box = new DataBox(dataPtr, areaPitch, Data.Length);
                            ResourceRegion region = new ResourceRegion();
                            region.Top = rect.Y;
                            region.Front = 0;
                            region.Back = 1;
                            region.Bottom = rect.Bottom;
                            region.Left = rect.X;
                            region.Right = rect.Right;
                            pipe.Context.UpdateSubresource1(box, texture.UnderlyingResource, subLevel, region);
                        }
                        else
                        {
                            int x = 0;
                            int y = 0;
                            int w = Math.Max((int)texture.Width >> (int)MipLevel, 1);
                            int h = Math.Max((int)texture.Height >> (int)MipLevel, 1);
                            DataBox box = new DataBox(dataPtr, Pitch, arraySliceBytes);
                            ResourceRegion region = new ResourceRegion();
                            region.Top = y;
                            region.Front = 0;
                            region.Back = 1;
                            region.Bottom = y + h;
                            region.Left = x;
                            region.Right = x + w;
                            pipe.Context.UpdateSubresource1(box, texture.UnderlyingResource, subLevel, region);
                        }


                        pipe.Profiler.Current.UpdateSubresourceCount++;
                    }
                }
            });
        }
    }
}
