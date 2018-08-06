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
        AnimatedSprite _parent = null;
        ThreadedList<AnimatedSprite> _children;
        SpriteData _data;
        int _frameID;
        double _elapsed;

        Matrix3x2F _globalTransform = Matrix3x2F.Identity;
        Matrix3x2F _localTransform = Matrix3x2F.Identity;


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
            }

            // Update transforms
            _localTransform = Matrix3x2F.Scaling(_scale) * Matrix3x2F.Rotation(_rotation) * Matrix3x2F.Translation(_position);
            _globalTransform = _parent != null ? _parent._globalTransform * _localTransform : _localTransform;

            // TODO factor origin into local transform. i.e. adjust translation with something like: _position - (Source.width|height * origin)
            // OR factor in the parent's origin into the child's global transform so that it's parental pivot is the origin of it's parent.
            // Experiment with this.

            _children.ForInterlock(0, 1, (index, child) =>
            {
                child.UpdateInternal(time);
                return false;
            });
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

        /// <summary>
        /// Gets the global 2D transform of the current <see cref="AnimatedSprite"/> instance. This is the result of the sprite's local transform combined with it's parent's global transform (if any).
        /// </summary>
        public Matrix3x2F GlobalTransform => _globalTransform;

        /// <summary>
        /// Gets the local (pre-parented) 2D transform of the current <see cref="AnimatedSprite"/> instance.
        /// </summary>
        public Matrix3x2F LocalTransform => _localTransform;

        /// <summary>
        /// Gets or sets the parent <see cref="AnimatedSprite"/>. Setting this to null will remove the sprite's parent.
        /// </summary>
        public AnimatedSprite Parent
        {
            get => _parent;
            set
            {
                if(_parent != value)
                {
                    if (_parent == this)
                        throw new InvalidOperationException("Cannot set a sprite's parent to itself.");

                    // We're detaching, maintain current global position as local.
                    if (value == null)
                    {
                        _localTransform = _globalTransform;
                    }
                    else // We're not detaching, up date local and global transform immediately.
                    {
                        _localTransform = Matrix3x2F.Scaling(_scale) * Matrix3x2F.Rotation(_rotation) * Matrix3x2F.Translation(_position);
                        _globalTransform = value._globalTransform * _localTransform;
                    }                    
                }
            }
        }

        Scene IUpdatable.Scene { get; set; }
    }
}
