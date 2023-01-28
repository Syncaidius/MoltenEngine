using Newtonsoft.Json;

namespace Molten.Graphics
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteData
    {
        RectangleF _source;
        RectStyle _style = RectStyle.Default;

        public SpriteData()
        {

        }

        public SpriteData(ITexture2D texture, float arraySlice = 0) :
            this(texture, new RectangleF(0, 0, texture.Width, texture.Height), arraySlice)
        { }

        public SpriteData(ITexture2D texture, RectangleF sourceBounds, float arraySlice = 0)
        {
            Texture = texture;
            Source = sourceBounds;
            ArraySlice = arraySlice;
        }

        public ITexture2D Texture { get; set; }

        [JsonProperty]
        public ref RectangleF Source => ref _source;

        [JsonProperty()]
        public float ArraySlice { get; set; }

        [JsonProperty]
        public ref RectStyle Style => ref _style;
    }
}
