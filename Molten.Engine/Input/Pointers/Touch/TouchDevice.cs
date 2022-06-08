using Molten.Utility;

namespace Molten.Input
{
    public abstract class TouchDevice : PointingDevice<int>
    {
        /// <summary>
        /// The number of active touch points on the current <see cref="ITouchDevice"/>.
        /// </summary>
        public abstract int TouchPointCount { get; protected set; }

        protected override sealed int TranslateStateID(int idValue) => idValue;

        protected override sealed int GetStateID(ref PointerState<int> state) => state.ID;

        protected override bool GetIsDown(ref PointerState<int> state)
        {
            return state.Action == InputAction.Pressed || 
                state.Action == InputAction.Held || 
                state.Action == InputAction.Moved;
        }
    }
}
