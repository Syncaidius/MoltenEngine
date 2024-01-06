namespace Molten.Input;

public class InputAnalogStick : InputDeviceFeature
{
    /// <summary>
    /// Gets a <see cref="Vector2F"/> containing the raw value for each stick axis.
    /// </summary>
    public Vector2I RawValue => _rawValue;

    /// <summary>
    /// Gets the raw value for the stick's X axis.
    /// </summary>
    public float RawX => _rawValue.X;

    /// <summary>
    /// Gets the raw value for the stick's Y axis.
    /// </summary>
    public float RawY => _rawValue.Y;

    /// <summary>
    /// Gets a <see cref="Vector2F"/> containing the percentage value for each stick axis. The deadzone is taken into account when calculating this value.
    /// </summary>
    public Vector2F Value => _value;

    /// <summary>
    /// Gets the percentage value for the stick's X axis.
    /// </summary>
    public float X => _value.X;

    /// <summary>
    /// Gets the percentage value for the stick's Y axis.
    /// </summary>
    public float Y => _value.Y;

    /// <summary>
    /// Gets or sets the stick deadzone as a percentage of the stick's total value range along each axis. 
    /// The lower this value is set, the more sensitive the stick should be. 
    /// However, setting it too low may cause the stick to become sensitive to unwanted movement. <para/>
    /// </summary>
    public virtual Vector2F Deadzone
    {
        get => _deadzone;
        set
        {
            _deadzone = new Vector2F()
            {
                X = float.Clamp(value.X, 0, 1.0f),
                Y = float.Clamp(value.Y, 0, 1.0f),
            };
        }
    }


    Vector2I _rawValue;
    Vector2F _value;
    Vector2F _deadzone = new Vector2F(0.5f);
    int _maxValue;

    public InputAnalogStick(string name, int maxValue, string desc = "Analog Stick") :
        base(name, desc)
    {
        _maxValue = maxValue;
    }

    public override void ClearState()
    {
        _rawValue = Vector2I.Zero;
        _value = Vector2F.Zero;
    }

    public void SetValues(int xValue, int yValue)
    {
        _rawValue.X = int.Clamp(xValue, -_maxValue, _maxValue);
        _rawValue.Y = int.Clamp(yValue, -_maxValue, _maxValue);

        float deadX = _maxValue * _deadzone.X;
        float deadY = _maxValue * _deadzone.Y;

        float finalRangeX = _maxValue - deadX;
        float finalRangeY = _maxValue - deadY;

        if (_rawValue.X <= -deadX)
            _value.X = MathF.Min(0, _rawValue.X + deadX) / finalRangeX;
        else
            _value.X = MathF.Max(0, _rawValue.X - deadX) / finalRangeX;

        if (yValue <= -deadY)
            _value.Y = MathF.Min(0, _rawValue.Y + deadY) / finalRangeY;
        else
            _value.Y = MathF.Max(0, _rawValue.Y - deadY) / finalRangeY;
    }

    protected override void OnUpdate(Timing time) { }
}
