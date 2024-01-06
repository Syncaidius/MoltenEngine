using Molten.Collections;

namespace Molten.Graphics.Textures;

internal class BCContext : IPoolable
{
    internal float[] FDir = new float[4];

    // Calculate new steps
    internal Color4[] PSteps = new Color4[4];

    public void ClearForPool() { }
}
