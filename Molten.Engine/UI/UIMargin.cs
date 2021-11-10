using System.Runtime.Serialization;

namespace Molten.UI
{
    [DataContract]
    public class UIMargin
    {
        public event ObjectHandler<UIMargin> OnChanged;

        bool _dockLeft, _dockTop, _dockRight, _dockBottom;
        int _left, _top, _right, _bottom;

        public void SetDock(bool left, bool top, bool right, bool bottom)
        {
            _dockLeft = left;
            _dockTop = top;
            _dockRight = right;
            _dockBottom = bottom;
            OnChanged?.Invoke(this);
        }

        public void SetMargin(int left, int top, int right, int bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
            OnChanged?.Invoke(this);
        }

        [DataMember]
        public bool DockLeft
        {
            get { return _dockLeft; }
            set
            {
                _dockLeft = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public bool DockRight
        {
            get { return _dockRight; }
            set
            {
                _dockRight = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public bool DockTop
        {
            get { return _dockTop; }
            set
            {
                _dockTop = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public bool DockBottom
        {
            get { return _dockBottom; }
            set
            {
                _dockBottom = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public int Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public int Right
        {
            get { return _right; }
            set
            {
                _right = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public int Top
        {
            get { return _top; }
            set
            {
                _top = value;
                OnChanged?.Invoke(this);
            }
        }

        [DataMember]
        public int Bottom
        {
            get { return _bottom; }
            set
            {
                _bottom = value;
                OnChanged?.Invoke(this);
            }
        }
    }
}
