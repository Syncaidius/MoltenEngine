namespace Molten.Input
{
    public class InputAnalogTrigger : InputDeviceFeature
    {
        /// <summary>
        /// Gets the raw value of the trigger. This may differ depending on the device being used.
        /// </summary>
        public float RawValue => _rawValue;

        /// <summary>
        /// Gets the trigger value as a percentage of the maximum value. 
        /// </summary>
        public float Value => _value;

        /// <summary>
        /// Gets the trigger deadzone, as a percentage.
        /// </summary>
        public float Deadzone { get; set; }

        int _maxValue;
        float _rawValue;
        float _value;
        float _deadzone;

        public InputAnalogTrigger(string name, int maxValue, string desc = "Analog Trigger") :
            base(name, desc)
        {
            _maxValue = maxValue;
            _deadzone = 0;
        }

        public void SetValue(float newValue)
        {
            _rawValue = float.Clamp(newValue, -_maxValue, _maxValue);

            double dead = _maxValue * _deadzone;
            double finalRange = _maxValue - dead;

            if (_rawValue <= -dead)
                _value = (float)(Math.Min(0, _rawValue + dead) / finalRange);
            else
                _value = (float)(Math.Max(0, _rawValue - dead) / finalRange);
        }


        public override void ClearState()
        {
            _value = 0;
            _rawValue = 0;
        }

        protected override void OnUpdate(Timing time) { }
    }
}
