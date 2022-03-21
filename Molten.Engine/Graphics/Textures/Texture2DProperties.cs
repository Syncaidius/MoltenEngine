namespace Molten.Graphics
{
    public class Texture2DProperties : Texture1DProperties
    {
        public uint Height = 1;

        public AntiAliasLevel MultiSampleLevel =  AntiAliasLevel.None;

        /// <summary>
        /// Sets <see cref="ArraySize"/> by multiplying the provided value by 6 (the number of sides/slices per cubemap).
        /// </summary>
        /// <param name="cubeCount">The number of cube maps to store in the texture. A number greater than 1 will form a cubemap array texture.</param>
        public void SetCubeCount(uint cubeCount)
        {
            ArraySize = cubeCount * 6U;
        }
    }
}
