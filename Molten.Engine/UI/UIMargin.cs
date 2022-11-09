using System.Runtime.Serialization;
using Molten.Graphics;
using Newtonsoft.Json.Linq;

namespace Molten.UI
{
    public class UIMargin
    {
        public delegate void OnChangedHandler(UIMargin margin, Side side);

        /// <summary>
        /// Invoked when any of the side properties are changed on the current <see cref="UIMargin"/>.
        /// </summary>
        public event OnChangedHandler OnChanged;

        public class Side
        {
            /// <summary>
            /// Invoked when either <see cref="Value"/> or <see cref="Mode"/> are changed.
            /// </summary>
            public event ObjectHandler<Side> OnChanged;

            int _value;
            SideMode _mode;

            /// <summary>
            /// Gets or sets the value of the current side. A value greater than zero will automatically set the <see cref="Mode"/> to <see cref="SideMode.Absolute"/>.
            /// </summary>
            public int Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnChanged?.Invoke(this);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the <see cref="SideMode"/> of the current margin <see cref="Side"/>. If set to <see cref="SideMode.Docked"/>, <see cref="Value"/> is ignored.
            /// </summary>
            public SideMode Mode
            {
                get => _mode;
                set
                {
                    if (_value != Value)
                    {
                        _value = Value;
                        OnChanged?.Invoke(this);
                    }
                }
            }
        }

        public enum SideMode
        {
            /// <summary>
            /// No margin for the current side.
            /// </summary>
            None = 0,

            /// <summary>
            /// An absolute-value margin. This means the margin will try to keep other elements at least the given distance away from 
            /// </summary>
            Absolute = 1,

            /// <summary>
            /// The margin will be auto-adjusted in relation to the side opposite the current one, to maintain the length of the current <see cref="UIElement"/> dimension.
            /// </summary>
            Auto = 2,
        }

        internal UIMargin()
        {
            Left = new Side();
            Top = new Side();
            Right = new Side();
            Bottom = new Side();

            Left.OnChanged += Side_OnChanged;
            Top.OnChanged += Side_OnChanged;
            Right.OnChanged += Side_OnChanged;
            Bottom.OnChanged += Side_OnChanged;
        }

        private void Side_OnChanged(Side side)
        {
            OnChanged?.Invoke(this, side);
        }

        /// <summary>
        /// Gets the left margin for the current <see cref="UIMargin"/>.
        /// </summary>
        public Side Left { get; }

        /// <summary>
        /// Gets the top margin for the current <see cref="UIMargin"/>.
        /// </summary>
        public Side Top { get; }

        /// <summary>
        /// Gets the right margin for the current <see cref="UIMargin"/>.
        /// </summary>
        public Side Right { get; }

        /// <summary>
        /// Gets the bottom margin for the current <see cref="UIMargin"/>.
        /// </summary>
        public Side Bottom { get; }
    }
}
