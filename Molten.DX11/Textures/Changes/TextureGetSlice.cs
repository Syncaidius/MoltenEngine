using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureGetSlice : ITextureChange
    {
        public TextureBase StagingTexture;

        public Action<TextureData.Slice> Callback;

        public int MipMapLevel;

        public int ArrayIndex;

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            if (!StagingTexture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(StagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (StagingTexture.Width != texture.Width ||
                StagingTexture.Height != texture.Height ||
                StagingTexture.Depth != texture.Depth)
                throw new TextureCopyException(texture, StagingTexture, "Staging texture dimensions do not match current texture.");

            if (MipMapLevel >= texture.MipMapCount)
                throw new TextureCopyException(texture, StagingTexture, "mip-map level must be less than the total mip-map levels of the texture.");

            if (ArrayIndex >= texture.ArraySize)
                throw new TextureCopyException(texture, StagingTexture, "array slice must be less than the array size of the texture.");

            StagingTexture.ApplyChanges(pipe);

            TextureData.Slice slice = texture.GetSliceData(pipe, StagingTexture, MipMapLevel, ArrayIndex, true);

            // Return resulting data
            Callback(slice);
        }
    }
}