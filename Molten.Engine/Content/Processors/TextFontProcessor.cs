using Molten.Graphics;

namespace Molten.Content
{
    public class TextFontProcessor : ContentProcessor<TextFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(TextFont), typeof(TextFontSource) };

        public override Type[] RequiredServices => null;

        public override Type PartType => typeof(TextFontSource);

        protected override bool OnReadPart(ContentLoadHandle handle, Stream stream, TextFontParameters parameters, object existingPart, out object partAsset)
        {
            partAsset = handle.Manager.Engine.Fonts.GetFont(stream, handle.Manager.Log, handle.RelativePath);
            return true;
        }

        protected override bool OnBuildAsset(ContentLoadHandle handle, ContentLoadHandle[] parts, TextFontParameters parameters, object existingAsset, out object asset)
        {
            if (parts.Length > 1)
                handle.LogWarning($"{nameof(TextFontProcessor)} does not support multi-part font loading. Using first part only.");

            TextFontSource tfs = parts[0].Get<TextFontSource>();
            asset = null;

            if (parts[0] != null)
            {
                if (handle.ContentType == typeof(TextFont))
                    asset = new TextFont(tfs, parameters.FontSize);
                else if (handle.ContentType == typeof(TextFontSource))
                    asset = tfs;

                return true;
            }

            return false;
        }

        protected override bool OnWrite(ContentHandle handle, Stream stream, TextFontParameters parameters, object asset)
        {
            throw new NotImplementedException();
        }
    }
}
