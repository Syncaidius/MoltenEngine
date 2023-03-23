namespace Molten.Graphics
{
    internal struct TextureGetSliceTask : IGraphicsResourceTask
    {
        public TextureDX11 Staging;

        public Action<TextureSlice> CompleteCallback;

        public uint MipMapLevel;

        public uint ArrayIndex;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            TextureDX11 texture = resource as TextureDX11;

            bool isStaging = texture.Flags.Has(GraphicsResourceFlags.AllReadWrite);
            bool stagingValid = Staging.Flags.Has(GraphicsResourceFlags.AllReadWrite);

            if (!stagingValid && !isStaging)
                throw new TextureFlagException(Staging.Flags, "Provided staging texture does not have the staging flag set.");

            // If the source texture is a staging texture itself, we don't need to use the provided staging texture.
            if (!isStaging)
            {
                // Validate dimensions.
                if (Staging.Width != texture.Width ||
                    Staging.Height != texture.Height ||
                    Staging.Depth != texture.Depth)
                    throw new TextureCopyException(texture, Staging, "Staging texture dimensions do not match current texture.");

                if (MipMapLevel >= texture.MipMapCount)
                    throw new TextureCopyException(texture, Staging, "mip-map level must be less than the total mip-map levels of the texture.");

                if (ArrayIndex >= texture.ArraySize)
                    throw new TextureCopyException(texture, Staging, "array slice must be less than the array size of the texture.");

                Staging.Apply(cmd);
            }
            else
            {
                Staging = null;
            }

            TextureSlice slice = texture.OnGetSliceData(cmd, Staging, MipMapLevel, ArrayIndex);

            // Return resulting data
            CompleteCallback?.Invoke(slice);
            return false;
        }
    }
}
