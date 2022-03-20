using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public class TextureData : ICloneable
    {
        /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
        public class Slice
        {
            public byte[] Data;

            public uint Pitch;
            public uint TotalBytes;

            public uint Width;
            public uint Height;

            public Slice Clone()
            {
                Slice result = new Slice()
                {
                    Data = new byte[this.Data.Length],
                    Pitch = this.Pitch,
                    TotalBytes = this.TotalBytes,
                    Width = this.Width,
                    Height = this.Height,
                };

                Array.Copy(Data, result.Data, TotalBytes);

                return result;
            }
        }

        public uint Width;
        public uint Height;
        public uint MipMapLevels;
        public uint ArraySize = 1;
        public uint SampleCount;

        /// <summary>The most detailed mip map level. by default, this is 0.</summary>
        public uint HighestMipMap = 0;

        public Slice[] Levels;
        public GraphicsFormat Format;
        public TextureFlags Flags;

        public bool IsCompressed;

        /// <summary>Decompresses the texture data to R8-G8-B8-A8 color format, if it is stored in a compressed format. This has no effect if already uncompressed.</summary>
        public void Decompress(Logger log)
        {
            BCHelper.Decompress(this, log);
        }

        public void Compress(DDSFormat format, Logger log)
        {
            BCHelper.Compress(this, format, log);
        }

        /// <summary>
        /// Attempts to convert the data held by the current <see cref="TextureData"/> instance into RGBA data.
        /// </summary>
        public void ToRGBA(Logger log)
        {
            if (IsCompressed)
                BCHelper.Decompress(this, log);

            if (Format == GraphicsFormat.B8G8R8A8_UNorm || Format == GraphicsFormat.B8G8R8A8_Typeless || Format == GraphicsFormat.B8G8R8A8_UNorm_SRgb)
            {
                foreach (Slice s in Levels)
                {
                    byte temp = 0;
                    for (uint i = 0; i < s.Data.Length; i += 4)
                    {
                        temp = s.Data[i];
                        s.Data[i] = s.Data[i + 2];
                        s.Data[i + 2] = temp;
                    }
                }
            }
        }

        public static uint GetLevelID(uint mipMapCount, uint targetMip, uint targetArraySlice)
        {
            return (targetArraySlice * mipMapCount) + targetMip;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>Sets the configuration and data of the current <see cref="TextureData"/> instance to that of the provided <see cref="TextureData"/> instance. <para/>
        /// This will overwrite as many texture slices as required to fit the other <see cref="TextureData"/> instance's data. 
        /// Any extra slices left over from the operation will remain untouched.</summary>
        /// <param name="otherData"></param>
        public void Set(TextureData otherData)
        {
            ArraySize = otherData.ArraySize;
            Format = otherData.Format;
            Flags = otherData.Flags;
            IsCompressed = otherData.IsCompressed;
            Height = otherData.Height;
            MipMapLevels = otherData.MipMapLevels;
            Width = otherData.Width;
            SampleCount = otherData.SampleCount;
            HighestMipMap = otherData.HighestMipMap;

            if (Levels.Length != otherData.Levels.Length)
                Array.Resize(ref Levels, otherData.Levels.Length);

            for (uint i = 0; i < otherData.Levels.Length; i++)
                Levels[i] = otherData.Levels[i].Clone();
        }

        /// <summary>Sets the specified <see cref="TextureData"/> as an array slice in the current <see cref="TextureData"/> instance. <para/>
        /// This will automatically increase the <see cref="ArraySize"/> on the current <see cref="TextureData"/> instance if needed.</summary>
        /// <param name="otherData">The other data to be appended.</param>
        /// <param name="arraySlice">The array slice at which to start copying the other data to.</param>
        public void Set(TextureData otherData, uint arraySlice)
        {
            if (otherData.Width != Width || otherData.Height != Height || otherData.MipMapLevels != MipMapLevels)
                throw new Exception("Texture data must match the dimensions (i.e. width, height, depth, mip-map levels) of the destination data.");

            uint start = MipMapLevels * arraySlice;
            uint end = start + (MipMapLevels * otherData.ArraySize);
            if (Levels == null || end >= Levels.Length)
            {
                EngineUtil.ArrayResize(ref Levels, end);
                ArraySize = Math.Max(ArraySize, arraySlice + otherData.ArraySize);
            }

            for (uint i = 0; i < otherData.ArraySize; i++)
            {
                for (uint j = 0; j < MipMapLevels; j++)
                {
                    uint sourceID = (MipMapLevels * i) + j;
                    uint destID = ((arraySlice + i) * MipMapLevels) + j;
                    Levels[destID] = otherData.Levels[sourceID].Clone();
                }
            }
        }


        /// <summary>Creates an exact copy of the texture data and returns the new instance.</summary>
        /// <returns></returns>
        public TextureData Clone()
        {
            TextureData result = new TextureData()
            {
                ArraySize = this.ArraySize,
                Format = this.Format,
                Flags = this.Flags,
                IsCompressed = this.IsCompressed,
                Height = this.Height,
                Levels = new Slice[this.Levels.Length],
                MipMapLevels = this.MipMapLevels,
                Width = this.Width,
                SampleCount = this.SampleCount,
                HighestMipMap = this.HighestMipMap,
            };

            // Copy mip-map level data.
            for (uint i = 0; i < Levels.Length; i++)
                result.Levels[i] = Levels[i].Clone();

            return result;
        }
    }
}
