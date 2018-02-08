using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Molten
{
    [DataContract]
    public class UIPadding
    {
        public event ObjectHandler<UIPadding> OnChanged;

        int _left, _right, _top, _bottom;

        public UIPadding(int margin) : this(margin, margin, margin, margin) { }

        public UIPadding(int left, int top, int right, int bottom)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        public UIPadding() : this(0, 0, 0, 0) { }

        public Rectangle ApplyPadding(Rectangle source)
        {
            source.X += _left;
            source.Y += _top;
            source.Width -= _left + _right;
            source.Height -= _top + _bottom;

            if (source.X < 0)
            {
                source.Width = Math.Max(source.Width + source.X, 0);
                source.X = 0;
            }

            if (source.Y < 0)
            {
                source.Height = Math.Max(source.Height + source.Y, 0);
                source.Y = 0;
            }

            source.X = Math.Max(source.X, 0);
            source.Y = Math.Max(source.Y, 0);
            source.Width = Math.Max(source.Width, 0);
            source.Height = Math.Max(source.Height, 0);

            return source;
        }

        public void Set(int left, int top, int right, int bottom)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;

            if(!SuppressEvents)
                OnChanged?.Invoke(this);
        }

        [DataMember]
        public int Left
        {
            get { return _left; }
            set
            {
                _left = value;
                if (!SuppressEvents)
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
                if (!SuppressEvents)
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
                if (!SuppressEvents)
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
                if (!SuppressEvents)
                    OnChanged?.Invoke(this);
            }
        }

        /// <summary>Gets or sets whether or not events on the <see cref="UIPadding"/> instance are suppressed when changing values.</summary>
        public bool SuppressEvents { get; set; }
    }
}
