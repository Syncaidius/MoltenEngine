using OpenGL;

namespace Molten.Graphics
{
    internal class GraphicsOpenGLFeatures : GraphicsDeviceFeatures
    {
        internal GraphicsOpenGLFeatures()
        {
            int maxTexSize;

            Gl.Get(GetPName.MaxTextureSize, out maxTexSize);
            MaxTextureDimension = maxTexSize;


            //int vTest = GL.GetInteger(GetPName.MaxTextureUnits); // Multi-texturing image units.
            //int test = GL.GetInteger(GetPName.MaxCombinedTextureImageUnits);
        }
    }
}
