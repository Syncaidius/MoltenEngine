using Molten.Graphics.Textures;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class TextureData : ICloneable
    {
        /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
        public class Slice
        {
            public byte[] Data;

            public int Pitch;
            public int TotalBytes;

            public int Width;
            public int Height;

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

        public int Width;
        public int Height;
        public int MipMapLevels;
        public int ArraySize;
        public int SampleCount;

        /// <summary>The most detailed mip map level. by default, this is 0.</summary>
        public int HighestMipMap = 0;

        public Slice[] Levels;
        public GraphicsFormat Format;
        public TextureFlags Flags;

        public bool IsCompressed;

        /// <summary>Decompresses the texture data to R8-G8-B8-A8 color format, if it is stored in a compressed format. This has no effect if already uncompressed.</summary>
        public void Decompress()
        {
            DDSHelper.Decompress(this);
        }

        public void Compress(DDSFormat format)
        {
            DDSHelper.Compress(this, format);
        }

        public static int GetLevelID(int mipMapCount, int targetMip, int targetArraySlice)
        {
            return (targetArraySlice * mipMapCount) + targetMip;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>Sets the configuration and data of the current <see cref="TextureData"/> instance to that of the provided <see cref="TextureData"/> instance.</summary>
        /// <param name="data"></param>
        public void Set(TextureData data)
        {
            ArraySize = data.ArraySize;
            Format = data.Format;
            Flags = data.Flags;
            IsCompressed = data.IsCompressed;
            Height = data.Height;
            MipMapLevels = data.MipMapLevels;
            Width = data.Width;
            SampleCount = data.SampleCount;

            if (Levels.Length != data.Levels.Length)
                Array.Resize(ref Levels, data.Levels.Length);

            for (int i = 0; i < data.Levels.Length; i++)
                Levels[i] = data.Levels[i].Clone();
        }

        /// <summary>Uses another <see cref="TextureData"/> instance to set texture data for the specified array slice of the current <see cref="TextureData"/> instance.</summary>
        /// <param name="data"></param>
        /// <param name="arraySlice"></param>
        public void Set(TextureData data, int arraySlice)
        {
            if (data.Width != Width || data.Height != Height || data.MipMapLevels != MipMapLevels)
                throw new Exception("Texture data must match the dimensions (i.e. width, height, depth, mip-map levels) of the destination data.");

            int requiredLevels = (MipMapLevels * (arraySlice + 1));

            if (Levels == null || ArraySize <= arraySlice || Levels.Length < requiredLevels)
                Array.Resize(ref Levels, requiredLevels);

            for(int i = 0; i < data.Levels.Length; i++)
            {
                int lID = (arraySlice * MipMapLevels) + i;
                Levels[lID] = data.Levels[i].Clone();
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
            };

            // Copy mip-map level data.
            for (int i = 0; i < Levels.Length; i++)
                result.Levels[i] = Levels[i].Clone();

            return result;
        }
    }
}
