using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteData
    {
        RectangleF _source;

        public ITexture2D Texture { get; set; }

        [JsonProperty]
        public ref RectangleF Source => ref _source;

        [JsonProperty()]
        public float ArraySlice { get; set; }
    }
}
