using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
                if(_left != value)
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
    }
}
