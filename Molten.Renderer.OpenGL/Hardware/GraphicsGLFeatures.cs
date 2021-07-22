using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    internal class GraphicsOpenGLFeatures : GraphicsDeviceFeatures
    {
        internal GraphicsOpenGLFeatures()
        {
            int maxTexSize;

            GL.GetInteger(GetPName.MaxTextureSize, out maxTexSize);
            MaxTextureDimension = maxTexSize;

              
            //int vTest = GL.GetInteger(GetPName.MaxTextureUnits); // Multi-texturing image units.
            //int test = GL.GetInteger(GetPName.MaxCombinedTextureImageUnits);
        }
    }
}
