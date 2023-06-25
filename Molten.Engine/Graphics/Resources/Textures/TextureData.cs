using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public unsafe class TextureData : ICloneable
    {
        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public uint Depth { get; private set; }

        public uint MipMapLevels { get; private set; }

        public uint ArraySize { get; private set; }

        public AntiAliasLevel MultiSampleLevel = AntiAliasLevel.None;

        public MSAAQuality MultiSampleQuality = MSAAQuality.Default;

        /// <summary>The most detailed mip map level. by default, this is 0.</summary>
        public uint HighestMipMap = 0;

        public TextureSlice[] Levels;
        public GraphicsFormat Format;
        public GraphicsResourceFlags Flags = GraphicsResourceFlags.None;

        public bool IsCompressed;

        public TextureData(uint width, uint height, uint depth, uint mipMapLevels, uint arraySize)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipMapLevels = mipMapLevels;
            ArraySize = arraySize;

            Levels = new TextureSlice[mipMapLevels * arraySize];
        }

        public TextureData(uint arraySize, params TextureSlice[] slices)
        {
            Width = slices[0].Width;
            Height = slices[0].Height;
            Depth = slices[0].Depth;
            MipMapLevels = (uint)(slices.Length / arraySize);
            ArraySize = arraySize;

            Levels = slices;
        }

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
                foreach (TextureSlice s in Levels)
                {
                    byte temp = 0;
                    for (uint i = 0; i < s.TotalBytes; i += 4)
                    {
                        temp = s.Data[i];
                        s.Data[i] = s.Data[i + 2];
                        s.Data[i + 2] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the subresource ID for the specified mip-map level and array slice, based on <see cref="MipMapLevels"/>.
        /// </summary>
        /// <param name="targetMip"></param>
        /// <param name="targetArraySlice"></param>
        /// <returns></returns>
        public uint GetLevelID(uint targetMip, uint targetArraySlice)
        {
            return (targetArraySlice * MipMapLevels) + targetMip;
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
            MultiSampleLevel = otherData.MultiSampleLevel;
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
            TextureData result = new TextureData(Width, Height, Depth, MipMapLevels, ArraySize)
            {
                Format = Format,
                Flags = Flags,
                IsCompressed = IsCompressed,
                Levels = new TextureSlice[Levels.Length],
                MultiSampleLevel = MultiSampleLevel,
                HighestMipMap = HighestMipMap,
            };

            // Copy mip-map level data.
            for (uint i = 0; i < Levels.Length; i++)
                result.Levels[i] = Levels[i].Clone();

            return result;
        }
    }
}
