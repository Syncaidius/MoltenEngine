using System.Runtime.Serialization;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// Represents padding for each side of a <see cref="UIElement"/>.
    /// </summary>
    public class UIPadding
    {
        public event ObjectHandler<UIPadding> OnChanged;

        int _left, _right, _top, _bottom;

        /// <summary>
        /// Gets the left side value.
        /// </summary>
        [DataMember]
        public int Left
        {
            get => _left;
            set
            {
                value = value < 0 ? 0 : value;
                if (_left != value)
                {
                    _left = value;
                    OnChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets the right side value.
        /// </summary>
        [DataMember]
        public int Right
        {
            get => _right;
            set
            {
                value = value < 0 ? 0 : value;
                if (_right != value)
                {
                    _right = value;
                    OnChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets the top side value.
        /// </summary>
        [DataMember]
        public int Top
        {
            get => _top;
            set
            {
                value = value < 0 ? 0 : value;
                if (_top != value)
                {
                    _top = value; 
                    OnChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets the bottom side value.
        /// </summary>
        [DataMember]
        public int Bottom
        {
            get => _bottom;
            set
            {
                value = value < 0 ? 0 : value;

                if (_bottom != value)
                {
                    _bottom = value; 
                    OnChanged?.Invoke(this);
                }
            }
        }

        public UIPadding() : this(0) { }

        public UIPadding(int value)
        {
            _left = _top = _right = _bottom = value;
        }

        public UIPadding(int leftRight, int topBottom)
        {
            _left = _right = leftRight;
            _top = _bottom = topBottom;
        }

        public UIPadding(int left, int top, int right, int bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        public virtual void Apply(UIPadding other)
        {
            _left = other._left;
            _top = other._top;
            _right = other._right;
            _bottom = other._bottom;
        }

        public Thickness ToThickness()
        {
            return new Thickness(_left, _top, _right, _bottom);
        }
    }
}
