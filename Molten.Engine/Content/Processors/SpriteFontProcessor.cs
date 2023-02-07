using Molten.Graphics;

namespace Molten.Content
{
    public class SpriteFontProcessor : ContentProcessor<SpriteFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(SpriteFont) };

        public override Type[] RequiredServices { get; } = new Type[] { typeof(RenderService) };

        public override Type PartType => typeof(SpriteFont);

        protected override bool OnReadPart(ContentLoadHandle handle, Stream stream, SpriteFontParameters parameters, object existingPart, out object partAsset)
        {
            RenderService renderer = handle.Manager.Engine.Renderer;
            partAsset = renderer.Fonts.GetFont(handle.RelativePath, parameters.FontSize);

            if (renderer.Fonts == null)
            {
                handle.LogError($"Unable to load. Renderer does not provide a font manager. RenderService.Fonts is null.");
                return false;
            }

            if(partAsset == null)
                partAsset = renderer.Fonts.LoadFont(stream, handle.RelativePath, parameters.FontSize);

            return true;
        }

        protected override bool OnBuildAsset(ContentLoadHandle handle, ContentLoadHandle[] parts, SpriteFontParameters parameters, object existingAsset, out object asset)
        {
            if (parts.Length > 1)
                handle.LogWarning($"{nameof(SpriteFontProcessor)} does not support multi-part font loading. Using first part only.");

            SpriteFont sf = parts[0].Get<SpriteFont>();
            asset = null;

            if (sf != null)
            {
                asset = sf;
                return true;
            }

            return false;
        }

        protected override bool OnWrite(ContentHandle handle, Stream stream, SpriteFontParameters parameters, object asset)
        {
            throw new NotImplementedException();
        }
    }
}
