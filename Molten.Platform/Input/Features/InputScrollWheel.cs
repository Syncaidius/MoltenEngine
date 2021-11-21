using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class InputScrollWheel : InputDeviceFeature
    {
        public override string Name { get; }

        public override string Description { get; }

        /// <summary>
        /// Gets the current position of the wheel.
        /// </summary>
        public int Position { get; private set; }

      
        /// <summary>
        /// Gets the delta of the wheel between the last and current update.
        /// </summary>
        public int Delta { get; private set; }

        public InputScrollWheel(string name, string desc = "Scroll Wheel")
        {
            Name = name;
            Description = desc;
        }

        public void SetValues(int position, int delta)
        {
            Position = position;
            Delta = delta;
        }

        public override void ClearState()
        {
            Position = 0;
            Delta = 0;
        }

        protected override void OnUpdate(Timing time) { }
    }
}
