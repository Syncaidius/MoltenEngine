﻿namespace Molten.Graphics
{
    public class TextureProperties
    {
        public TextureProperties(GraphicsTextureType type)
        {
            TextureType = type;
        }

        public TextureDimensions Dimensions = new TextureDimensions()
        {
            Width = 1,
            Height = 1,
            Depth = 1,
            MipMapLevels = 1,
            ArraySize = 1,
        };

        public GraphicsTextureType TextureType;

        public GraphicsFormat Format = GraphicsFormat.R8G8B8A8_UNorm;

        public GraphicsResourceFlags Flags = GraphicsResourceFlags.GpuWrite;

        public AntiAliasLevel MultiSampleLevel = AntiAliasLevel.None;

        public MSAAQuality SampleQuality = MSAAQuality.Default;

        public string Name = null;

        /// <summary>
        /// Sets <see cref="Dimensions"/> array size by multiplying the provided value by 6 (the number of sides/slices per cubemap).
        /// </summary>
        /// <param name="cubeCount">The number of cube maps to store in the texture. A number greater than 1 will form a cubemap array texture.</param>
        public void SetCubeCount(uint cubeCount)
        {
            Dimensions.ArraySize = cubeCount * 6U;
        }

        public uint Width
        {
            get => Dimensions.Width;
            set => Dimensions.Width = value;
        }

        public uint Height
        {
            get => Dimensions.Height;
            set => Dimensions.Height = value;
        }

        public uint Depth
        {
            get => Dimensions.Depth;
            set => Dimensions.Depth = value;
        }

        public uint MipMapLevels
        {
            get => Dimensions.MipMapLevels;
            set => Dimensions.MipMapLevels = value;
        }

        public uint ArraySize
        {
            get => Dimensions.ArraySize;
            set => Dimensions.ArraySize = value;
        }
    }
}
