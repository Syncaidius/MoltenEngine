using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
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

        public override string Name { get; }

        public override string Description { get; }

        public InputVibration(float maxValue, string name, string desc = "Vibrator")
        {
            Name = name;
            Description = desc;
            MaxValue = maxValue;
        }
        protected override void OnUpdate(Timing time) { }

        public override void ClearState()
        {
            Value = 0f;
        }
    }
}
