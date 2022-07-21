using Molten.Graphics;

namespace Molten.Content
{
    public class TextFontProcessor : ContentProcessor<SpriteFontParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(TextFont), typeof(TextFontSource) };

        public override Type[] RequiredServices => null;

        protected override bool OnRead(ContentHandle handle, SpriteFontParameters parameters, object existingAsset, out object asset)
        {
            TextFontSource tfs = handle.Manager.Engine.Fonts.GetFont(handle.Manager.Log, handle.Path);
            asset = null;

            if (tfs != null)
            {
                if (handle.ContentType == typeof(TextFont))
                    asset = new TextFont(tfs, parameters.FontSize);
                else if (handle.ContentType == typeof(TextFontSource))
                    asset = tfs;

                return true;
            }

            return false;
        }

        protected override bool OnWrite(ContentHandle handle, SpriteFontParameters parameters, object asset)
        {
            throw new NotImplementedException();
        }
    }
}
