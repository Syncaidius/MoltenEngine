using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteAnimation
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class KeyFrame
        {
            [JsonProperty]
            public string Key { get; set; }

            public SpriteData Data { get; set; }

            [JsonProperty]
            public Vector2F Scale { get; set; }

            [JsonProperty]
            public Vector2F Offset { get; set; }

            [JsonProperty]
            public float Rotation { get; set; }

            [JsonProperty]
            public Vector2F Origin { get; set; }

            /// <summary>
            /// The length of time the keyframe is held before moving onto the next, in milliseconds
            /// </summary>
            [JsonProperty]
            public double Time { get; set; }
        }

        SpriteSheet _sheet;

        public SpriteAnimation(SpriteSheet sheet)
        {
            Sheet = sheet;
        }

        public SpriteAnimation()
        {
            Sheet = null;
        }

        private void ValidateFrames()
        {
            ErrorFrame = -1;

            for(int i = 0; i < Frames.Count; i++)
            {
                KeyFrame f = Frames[i];

                if (_sheet.TryGetData(f.Key, out SpriteData data))
                {
                    f.Data = data;
                }
                else
                {
                    ErrorFrame = i;
                    break;
                }
            }
        }

        /// <summary>
        /// A list containing all of the animation frames
        /// </summary>
        [DataMember]
        public List<KeyFrame> Frames { get; set; } = new List<KeyFrame>();

        /// <summary>
        /// Gets or sets whether the animation should loop
        /// </summary>
        public bool IsLooped { get; set; } = true;

        /// <summary>
        /// Gets the ID of the frame that is has an error, or -1 if none.
        /// </summary>
        public int ErrorFrame { get; private set; } = -1;

        /// <summary>
        /// Gets the <see cref="SpriteSheet"/> that the current <see cref="SpriteAnimation"/> is bound to.
        /// </summary>
        public SpriteSheet Sheet
        {
            get => _sheet;
            set
            {
                if (_sheet != value)
                {
                    _sheet = value;

                    if (_sheet != null)
                        ValidateFrames();
                }
            }
        }
    }
}
