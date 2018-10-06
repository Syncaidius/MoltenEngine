using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureGet : ITextureChange
    {
        public TextureBase StagingTexture;

        public Action<TextureData> Callback;

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            if (!StagingTexture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(StagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (StagingTexture.Width != texture.Width ||
                StagingTexture.Height != texture.Height ||
                StagingTexture.Depth != texture.Depth)
                throw new TextureCopyException(texture, StagingTexture, "Staging texture dimensions do not match current texture.");

            StagingTexture.Apply(pipe);

            // Copy the texture into the staging texture.
            pipe.Context.CopyResource(texture.UnderlyingResource, StagingTexture.UnderlyingResource);

            TextureData data = new TextureData()
            {
                ArraySize = texture.ArraySize,
                Flags = texture.Flags,
                Format = texture.Format,
                Height = texture.Height,
                HighestMipMap = 0,
                IsCompressed = texture.IsBlockCompressed,
                Levels = new TextureData.Slice[texture.ArraySize * texture.MipMapCount],
                MipMapLevels = texture.MipMapCount,
                Width = texture.Width,
            };

            int levelID = 0;

            pipe.Context.CopyResource(texture.UnderlyingResource, StagingTexture.UnderlyingResource);

            // Iterate over each array slice.
            for (int a = 0; a < texture.ArraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (int i = 0; i < texture.MipMapCount; i++)
                {
                    levelID = (a * texture.MipMapCount) + i;
                    data.Levels[levelID] = StagingTexture.GetSliceData(pipe, null, i, a);
                }
            }

            // Return resulting data
            Callback(data);
        }
    }
}