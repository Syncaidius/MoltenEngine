using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public partial struct ComputeStateParameters
    {
        public void ApplyPreset(ComputeStatePreset preset)
        {
            // TODO apply preset.
        }
    }

    public enum ComputeStatePreset
    {
        Default = 0
    }
}
