namespace Molten.Graphics
{
    public struct TextureGetSliceTask : IGraphicsResourceTask
    {
        public GraphicsResource Staging;

        public Action<TextureSlice> CompleteCallback;

        public uint MipMapLevel;

        public uint ArrayIndex;

        public GraphicsMapType MapType;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            GraphicsTexture texture = resource as GraphicsTexture;
            GraphicsTexture texStaging = Staging as GraphicsTexture;

            bool isStaging = texture.Flags.Has(GraphicsResourceFlags.AllReadWrite);
            bool stagingValid = Staging.Flags.Has(GraphicsResourceFlags.AllReadWrite);

            if (!stagingValid && !isStaging)
                throw new GraphicsResourceException(Staging, "Provided staging texture does not have the staging flag set.");

            // If the source texture is a staging texture itself, we don't need to use the provided staging texture.
            if (!isStaging)
            {
                // Validate dimensions.
                if (texStaging.Width != texture.Width ||
                    texStaging.Height != texture.Height ||
                    texStaging.Depth != texture.Depth)
                    throw new ResourceCopyException(resource, Staging, "Staging texture dimensions do not match current texture.");

                if (MipMapLevel >= texture.MipMapCount)
                    throw new ResourceCopyException(resource, Staging, "mip-map level must be less than the total mip-map levels of the texture.");

                if (ArrayIndex >= texture.ArraySize)
                    throw new ResourceCopyException(resource, Staging, "array slice must be less than the array size of the texture.");

                Staging.Ensure(cmd);
            }
            else
            {
                Staging = null;
            }

            TextureSlice slice = TextureSlice.FromTextureSlice(cmd, texture, texStaging, MipMapLevel, ArrayIndex, MapType);

            // Return resulting data
            CompleteCallback?.Invoke(slice);
            return false;
        }
    }
}
