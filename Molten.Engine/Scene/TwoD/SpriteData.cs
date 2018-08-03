using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SpriteData
    {
        [Flags]
        public enum SourceTransitionFlags
        {
            /// <summary>
            /// The sprite source will instantly change to the next frame once the current frame's duration is surpassed.
            /// </summary>
            Instant = 0,
            
            /// <summary>The sprite source rectangle will be scrolled horizontally towards the next frame's location.</summary>
            HorizonalScroll = 1,

            /// <summary>The sprite source rectangle will be scrolled vertically towards the next frame's location.</summary>
            VerticalScroll = 2,
        }

        public enum RotationTransition
        {
            /// <summary>
            /// The sprite's rotation will transition instantly to the next frame once the current frame's duration is surpassed.
            /// </summary>
            Instant = 0,

            /// <summary>
            /// The sprite's rotation will gradually interpolate clockwise towards the rotation of the next frame.
            /// </summary>
            Clockwise = 1,

            /// <summary>
            /// The sprite's rotation will gradually interpolate counter-clockwise towards the rotation of the next frame.
            /// </summary>
            CounterClockwise = 2,
        }

        /// <summary>
        /// Represents the frame of a sprite animation data.
        /// </summary>
        public class Frame
        {
            /// <summary>
            /// The source rectangle for the current frame.
            /// </summary>
            public Rectangle Source;

            /// <summary>
            /// The duration of the frame and it's transition, if any.
            /// </summary>
            public float Duration;

            /// <summary>
            /// The transition flags of the current frame. The default value is <see cref="SourceTransitionFlags.Instant"/>.
            /// </summary>
            public SourceTransitionFlags SourceFlags = SourceTransitionFlags.Instant;

            /// <summary>
            /// The rotation mode of the current frame.
            /// </summary>
            public RotationTransition RotationMode = RotationTransition.Instant;

            /// <summary>
            /// The scale of the current frame.
            /// </summary>
            public Vector2F Scale;

            /// <summary>
            /// The origin of the current frame.
            /// </summary>
            public Vector2F Origin;
        }

        /// <summary>Gets a the list of animation frames. </summary>
        public List<Frame> Frames { get; private set; }

        public SpriteData()
        {
            Frames = new List<Frame>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="SpriteData"/> using an existing list of frames.
        /// </summary>
        /// <param name="frames"></param>
        public SpriteData(List<Frame> frames)
        {
            Frames = frames;
        }

        /// <summary>
        /// Creates a new <see cref="AnimatedSprite"/> instance with the current <see cref="SpriteData"/> as its data source.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public AnimatedSprite CreateSprite(Vector2F position)
        {
            return new AnimatedSprite(this)
            {
                Position = position,
            };
        }
    }
}
