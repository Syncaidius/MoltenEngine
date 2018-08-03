using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// An advanced sprite class capable of animation and parenting. It is updated every scene tick.
    /// </summary>
    public class AnimatedSprite : Sprite, IUpdatable
    {
        Scene IUpdatable.Scene { get; set; }

        SpriteData _data;
        int _frameID;
        double _elapsed;
        ThreadedList<AnimatedSprite> _children;

        /// <summary>
        /// Creates a new instance of animated sprite.
        /// </summary>
        /// <param name="data"></param>
        public AnimatedSprite(SpriteData data)
        {
            _data = data ?? throw new ArgumentNullException("Data cannot be null.");
            _children = new ThreadedList<AnimatedSprite>();
        }

        private void UpdateInternal(Timing time)
        {
            _elapsed += time.ElapsedTime.TotalMilliseconds;
            if (_elapsed >= _data.Frames[_frameID].Duration)
            {
                _elapsed -= _data.Frames[_frameID].Duration;
                _frameID++;
                if (_frameID == _data.Frames.Count)
                {
                    if (IsLooping)
                        _frameID = 0;
                    else
                        _frameID--;
                }

                Source = _data.Frames[_frameID].Source;

                _children.ForInterlock(0, 1, (index, child) =>
                {
                    // TODO implement local position, rotation and scale for AnimatedSprite.
                    // TODO update global child position based on parent global position + child local position.
                    child.UpdateInternal(time);
                    return false;
                });
            }
        }

        void IUpdatable.Update(Timing time)
        {
            UpdateInternal(time);
        }

        /// <summary>
        /// Gets or sets the frame of the current <see cref="Sprite"/> instance.
        /// </summary>
        public int CurrentFrame
        {
            get => _frameID;
            set
            {
                _frameID = MathHelper.Clamp(value, 0, Math.Max(0, _data.Frames.Count - 1));
                _elapsed = 0;
            }
        }

        /// <summary>
        /// Gets or sets whether the sprite will start it's animation again once completed.
        /// </summary>
        public bool IsLooping { get; set; }

        /// <summary>
        /// Gets or sets the animation speed. The default v alue is 1.0f.
        /// </summary>
        public float AnimationSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the <see cref="SpriteData"/> for the current <see cref="Sprite"/> instance.
        /// </summary>
        public SpriteData Data
        {
            get => _data;
            set
            {
                if (_data == null)
                    throw new InvalidOperationException("Data cannot be set to null.");

                if(_data != value)
                {
                    _elapsed = 0;
                    _data = value;
                    _frameID = Math.Min(Math.Max(0, _data.Frames.Count - 1), _frameID);
                }
            }
        }

        /// <summary>
        /// Gets a thread-safe list containing the child sprites of the current <see cref="AnimatedSprite"/>.
        /// </summary>
        public ThreadedList<AnimatedSprite> Children => _children;
    }
}
