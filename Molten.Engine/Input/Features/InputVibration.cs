namespace Molten.Input;

public class InputVibration : InputDeviceFeature
{
    /// <summary>
    /// Gets or sets the vibration intensity value.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets the maximum vibration intensity value.
    /// </summary>
    public float MaxValue { get; }

    public InputVibration(string name, float maxValue, string desc = "Vibrator") :
        base(name, desc)
    {
        MaxValue = maxValue;
    }
    protected override void OnUpdate(Timing time) { }

    public override void ClearState()
    {
        Value = 0f;
    }
}
