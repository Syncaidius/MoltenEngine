namespace Molten.Graphics
{
    public class Texture2DProperties : Texture1DProperties
    {
        public int Height = 1;

        public int SampleCount = 1;

        /// <summary>
        /// Sets <see cref="ArraySize"/> by multiplying the provided value by 6 (the number of sides/slices per cubemap).
        /// </summary>
        /// <param name="cubeCount">The number of cube maps to store in the texture. A number greater than 1 will form a cubemap array texture.</param>
        public void SetCubeCount(int cubeCount)
        {
            ArraySize = cubeCount * 6;
        }
    }
}
