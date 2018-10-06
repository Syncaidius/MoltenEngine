using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class AnimatedSprite : Sprite
    {
        SpriteAnimation _animation;
        SpriteAnimation.KeyFrame _curFrame;
        int _frame;
        double _frameTime;

        public AnimatedSprite()
        {

        }

        public AnimatedSprite(SpriteAnimation animation)
        {
            _animation = animation;
        }

        private void GetFrame()
        {
            _curFrame = _animation.Frames[Frame];

            Source = _curFrame.Source;
            ArraySlice = _curFrame.ArrayIndex;
        }

        public void Play(bool reset = false)
        {
            if (reset)
            {
                Frame = 0;
                _frameTime = 0;
            }

            if (_animation == null)
                return;

            State = SpriteAnimationState.Playing;
        }

        public void Pause()
        {
            State = SpriteAnimationState.Paused;
        }

        public void Stop()
        {
            State = SpriteAnimationState.Stopped;
            Frame = 0;
            _frameTime = 0;
        }

        public void Update(Timing time)
        {
            if (State == SpriteAnimationState.Stopped || _animation == null || _animation.Frames.Count == 0)
                return;

            _frameTime += time.ElapsedTime.TotalMilliseconds * AnimationSpeed;

            if (_frameTime >= _curFrame.Time)
            {
                _frame++;
                _frameTime = 0;

                if (_frame >= _animation.Frames.Count)
                {
                    if (IsLooping)
                    {
                        _frame = 0;
                    }
                    else
                    {
                        _frame = _animation.Frames.Count - 1;
                        State = SpriteAnimationState.Stopped;
                    }
                }

                GetFrame();
            }
        }

        /// <summary>
        /// Gets or sets the sprite animation of the current <see cref="AnimatedSprite"/> instance.
        /// </summary>
        public double AnimationSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets whether the current <see cref="AnimatedSprite"/>'s animation restarts from teh beginning once it reaches the end, or stops.<para/>
        /// The default value is true.
        /// </summary>
        public bool IsLooping { get; set; } = true;

        /// <summary>
        /// Gets or sets the frame number/ID of the current <see cref="AnimatedSprite"/> instance. <para/>
        /// The value will be constrained to a number between 0 and the maximum number of frames in <see cref="Animation"/>, or 0 if no animation is set.
        /// </summary>
        public int Frame
        {
            get => _frame;
            set
            {
                if (_animation != null)
                {
                    _frame = MathHelper.Clamp(value, 0, _animation.Frames.Count - 1);
                    GetFrame();
                }
                else
                {
                    _curFrame = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sprite animation of the current <see cref="AnimatedSprite"/> instance.
        /// </summary>
        public SpriteAnimation Animation
        {
            get => _animation;
            set
            {
                if(_animation != value)
                {
                    _animation = value;
                    Frame = 0;

                    if (_animation == null)
                        _curFrame = null;
                    else
                        GetFrame();
                }
            }
        }

        /// <summary>
        /// Gets the current animation state of the sprite.
        /// </summary>
        public SpriteAnimationState State { get; private set; }
    }

    public enum SpriteAnimationState
    {
        Stopped = 0,
        Paused = 1,
        Playing = 2,
    }

    [DataContract]
    public class SpriteAnimation
    {
        [DataContract]
        public class KeyFrame
        {
            /// <summary>
            /// The location of the sprite frame on a texture sheet.
            /// </summary>
            [DataMember]
            public Rectangle Source { get; set; }

            /// <summary>
            /// The texture array index containing the frame's source texture/sprite. This will be ignored if the sprite is rendered with a non-array texture.
            /// </summary>
            [DataMember]
            public int ArrayIndex { get; set; }

            /// <summary>
            /// The length of time the keyframe is held before moving onto the next, in milliseconds
            /// </summary>
            [DataMember]
            public double Time { get; set; }
        }

        /// <summary>
        /// A list containing all of the animation frames
        /// </summary>
        [DataMember]
        public List<KeyFrame> Frames { get; set; } = new List<KeyFrame>();
    }
}
