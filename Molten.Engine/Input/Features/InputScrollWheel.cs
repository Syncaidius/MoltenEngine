namespace Molten.Input
{
    public class InputScrollWheel : InputDeviceFeature
    {
        /// <summary>
        /// Gets the current position of the wheel.
        /// </summary>
        public int Position { get; private set; }

      
        /// <summary>
        /// Gets the delta of the wheel between the last and current update.
        /// </summary>
        public int Delta { get; private set; }

        public int Increment { get; set; } = 120;

        public int _prevPosition;

        public InputScrollWheel(string name, string desc = "Scroll Wheel") : base(name, desc) { }

        public void Move(int delta)
        {
            Position += Increment * (delta > 0 ? 1 : delta < 1 ? -1 : 0);
        }

        public override void ClearState()
        {
            Position = 0;
            Delta = 0;
        }

        protected override void OnUpdate(Timing time)
        {
            Delta = Position - _prevPosition;
            _prevPosition = Position;
        }
    }
}
