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
        public enum TransitionFlags
        {
            None = 0,
            
            /// <summary>The sprite source rectangle will be scrolled horizontally towards the next frame's location.</summary>
            HorizonalScroll = 1,

            /// <summary>The sprite source rectangle will be scrolled vertically towards the next frame's location.</summary>
            VerticalScroll = 2,
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
            /// The transition flags of the current frame. The default valeu is <see cref="TransitionFlags.None"/>.
            /// </summary>
            public TransitionFlags Transition;
        }

        /// <summary>Gets a the list of animation frames. </summary>
        public List<Frame> Frames { get; private set; }

        public SpriteData()
        {
            Frames = new List<Frame>();
        }

        public SpriteData(List<Frame> frames)
        {
            Frames = frames;
        }
    }
}
