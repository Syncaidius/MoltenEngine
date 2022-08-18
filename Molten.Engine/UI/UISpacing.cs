using System.Runtime.Serialization;
using Molten.Graphics;

namespace Molten.UI
{
    public class UISpacing
    {
        public event ObjectHandler OnChanged;

        int _left, _right, _top, _bottom;

        [DataMember]
        public int Left
        {
            get => _left;
            set
            {
                if (_left != value)
                {
                    _left = value;
                    OnChanged?.Invoke();
                }
            }
        }

        [DataMember]
        public int Right
        {
            get => _right;
            set
            {
                if (_right != value)
                {
                    _right = value;
                    OnChanged?.Invoke();
                }
            }
        }

        [DataMember]
        public int Top
        {
            get => _top;
            set
            {
                if (_top != value)
                {
                    _top = value;
                    OnChanged?.Invoke();
                }
            }
        }

        [DataMember]
        public int Bottom
        {
            get => _bottom;
            set
            {
                if (_bottom != value)
                {
                    _bottom = value;
                    OnChanged?.Invoke();
                }
            }
        }

        public UISpacing() : this(0) { }

        public UISpacing(int spacing)
        {
            _left = _top = _right = _bottom = spacing;
        }

        public UISpacing(int left, int top, int right, int bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        public void Apply(UISpacing other)
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
