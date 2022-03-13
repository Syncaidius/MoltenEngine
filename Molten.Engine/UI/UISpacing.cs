using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UISpacing
    {
        public event ObjectHandler OnChanged;

        int _left, _right, _top, _bottom;

        public int Left
        {
            get => _left;
            set
            {
                if(_left != value)
                {
                    _left = value;
                    OnChanged?.Invoke();
                }
            }
        }

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
    }
}
