using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal abstract class BCContext
    {
        internal float[] fDir = new float[4];

        // Calculate new steps
        internal HDRColorA[] pSteps = new HDRColorA[4];
    }
}
