namespace Molten.UI;

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
                if (_mode != value)
                {
                    _mode = value;
                    OnChanged?.Invoke(this);
                }
            }
        }
    }

    public enum SideMode
    {
        /// <summary>
        /// No margin for the current side. The current side will not be adjusted automatically.
        /// </summary>
        None = 0,

        /// <summary>
        /// An absolute-value margin. This means the margin will try to keep other elements at least the given distance away from 
        /// </summary>
        Absolute = 1,

        /// <summary>
        /// A percent-based margin. This means the margin will be equal to a percentage of the parent element's bounds size, along the respective axis.
        /// <para>
        /// The <see cref="Side.Value"/> must be a between 0 and 100 to represent a valid percentage. 
        /// Values outside this range will be automatically clamped during bounds calculations.
        /// </para>
        /// </summary>
        Percent = 2,
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

    /// <summary>
    /// Sets all properties of <see cref="UIMargin"/> so that the parent <see cref="UIElement"/> will always resize to fit its parent's bounds, if any.
    /// </summary>
    public void FitToParent()
    {
        Left.Mode = Right.Mode = Top.Mode = Bottom.Mode = SideMode.Absolute;
        Left.Value = Right.Value = Top.Value = Bottom.Value = 0;
    }

    internal void Apply(ref Rectangle bounds, Rectangle? parentBounds)
    {
        if (parentBounds.HasValue)
        {
            Rectangle pBounds = parentBounds.Value;
            ApplySide(Left, ref bounds.Left, pBounds.Left, pBounds.Width, 1);
            ApplySide(Right, ref bounds.Right, pBounds.Right, pBounds.Width, -1);
            ApplySide(Top, ref bounds.Top, pBounds.Top, pBounds.Height, 1);
            ApplySide(Bottom, ref bounds.Bottom, pBounds.Bottom, pBounds.Height, -1);
        }
        else
        {
            ApplySide(Left, ref bounds.Left, 0, 0, 1);
            ApplySide(Right, ref bounds.Right, 0, 0, -1);
            ApplySide(Top, ref bounds.Top, 0, 0, 1);
            ApplySide(Bottom, ref bounds.Bottom, 0, 0, -1);
        }
    }

    private void ApplySide(Side side, ref int boundsValue, int origin, int parentDimension, int direction)
    {
        switch (side.Mode)
        {
            case SideMode.Absolute:
                if (parentDimension > 0)
                    boundsValue = origin + (side.Value * direction);
                else
                    boundsValue += side.Value * direction;
                break;

            case SideMode.Percent:
                if (parentDimension > 0)
                {
                    int percent = int.Clamp(side.Value, 0, 100);

                    if (percent > 0)
                        boundsValue += parentDimension * (int)(100f / percent) * direction;
                }
                break;
        }
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
