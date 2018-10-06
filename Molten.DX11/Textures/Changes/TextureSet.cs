using Molten.Graphics.Textures;
using SharpDX;
using SharpDX.Direct3D11;
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

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            //texture.SetDataInternal<T>(pipe, Data, StartIndex, Count, Stride, MipLevel, ArrayIndex, Pitch, Area);
            //calculate size of a single level
            int sliceBytes = 0;
            int blockSize = 8; // default block size
            int levelWidth = texture.Width;
            int levelHeight = texture.Height;

            if (texture.IsBlockCompressed)
            {
                blockSize = DXTHelper.GetBlockSize(texture.Format);

                // Collect total level size.
                for (int i = 0; i < texture.MipMapCount; i++)
                {
                    sliceBytes += DXTHelper.GetBCLevelSize(levelWidth, levelHeight, blockSize);
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }
            else
            {
                for (int i = 0; i < texture.MipMapCount; i++)
                {
                    sliceBytes += levelWidth * levelHeight * 4; //4 color channels. 1 byte each. Width * height * colorByteSize.
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            //======DATA TRANSFER===========
            EngineInterop.PinObject(Data, (ptr) =>
            {
                int startBytes = StartIndex * Stride;
                IntPtr dataPtr = ptr + startBytes;
                int subLevel = (texture.MipMapCount * ArrayIndex) + MipLevel;

                if (texture.HasFlags(TextureFlags.Dynamic))
                {
                    DataStream stream = null;
                    DataBox destBox = pipe.Context.MapSubresource(
                        texture.UnderlyingResource, 
                        subLevel, 
                        MapMode.WriteDiscard, 
                        MapFlags.None, 
                        out stream);

                    pipe.Profiler.Current.MapDiscardCount++;

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
                            stream.WriteRange(dataPtr, areaPitch);

                            dataPtr += areaPitch;
                            aY++;
                        }
                    }
                    else
                    {
                        stream.WriteRange(dataPtr, Count);
                    }

                    pipe.Context.UnmapSubresource(texture.UnderlyingResource, subLevel);
                }
                else
                {
                    if (texture.IsBlockCompressed)
                    {
                        // Calculate mip-map level size.
                        levelWidth = texture.Width >> MipLevel;
                        levelHeight = texture.Height >> MipLevel;

                        int bcPitch = DXTHelper.GetBCPitch(levelWidth, levelHeight, blockSize);
                        DataBox box = new DataBox(dataPtr, bcPitch, sliceBytes);

                        if (Area != null)
                            throw new NotImplementedException("Area-based SetData on block-compressed texture is currently unsupported. Sorry!");

                        pipe.Context.UpdateSubresource(box, texture.UnderlyingResource, subLevel);
                        pipe.Profiler.Current.UpdateSubresourceCount++;
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
                            pipe.Context.UpdateSubresource(box, texture.UnderlyingResource, subLevel, region);
                        }
                        else
                        {
                            int x = 0;
                            int y = 0;
                            int w = Math.Max(texture.Width >> MipLevel, 1);
                            int h = Math.Max(texture.Height >> MipLevel, 1);

                            DataBox box = new DataBox(dataPtr, Pitch, sliceBytes);

                            ResourceRegion region = new ResourceRegion();
                            region.Top = y;
                            region.Front = 0;
                            region.Back = 1;
                            region.Bottom = y + h;
                            region.Left = x;
                            region.Right = x + w;

                            pipe.Context.UpdateSubresource(box, texture.UnderlyingResource, subLevel, region);
                        }
                    }
                }
            });
        }
    }
}
