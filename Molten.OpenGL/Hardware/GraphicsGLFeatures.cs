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
            MaxTextureDimension = Math.Min(GL.GetInteger(GetPName.MaxTextureSize), GL.GetInteger(GetPName.MaxRectangleTextureSize));
            MaxCubeMapDimension = GL.GetInteger(GetPName.MaxCubeMapTextureSize);
            SimultaneousRenderSurfaces = GL.GetInteger(GetPName.MaxDrawBuffers);

            MaxFragmentTextureImageUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            MaxVertexTextureImageUnits = GL.GetInteger(GetPName.MaxVertexTextureImageUnits);
            MaxGeometryTextureImageUnits = GL.GetInteger(GetPName.MaxGeometryTextureImageUnits);
            MaxTcsTextureImageUnits = GL.GetInteger(GetPName.MaxTessControlTextureImageUnits); // Equivilent to hull shaders in DX11+
            MaxTesTextureImageUnits = GL.GetInteger(GetPName.MaxTessEvaluationTextureImageUnits); // Equvilent to domain shaders in DX11+

              
            //int vTest = GL.GetInteger(GetPName.MaxTextureUnits); // Multi-texturing image units.
            //int test = GL.GetInteger(GetPName.MaxCombinedTextureImageUnits);
        }

        /// <summary>
        /// Gets the maximum number of texture image units supported by the vertex shader stage.
        /// </summary>
        internal int MaxVertexTextureImageUnits { get; private set; }

        /// <summary>
        /// Gets the maximum number of texture image units supported by the geometry shader stage.
        /// </summary>
        internal int MaxGeometryTextureImageUnits { get; private set; }

        /// <summary>
        /// Gets the maximum number of texture image units supported by the tessellation-control shader (TCS) stage.
        /// </summary>
        internal int MaxTcsTextureImageUnits { get; private set; }

        /// <summary>
        /// Gets the maximum number of texture image units supported by the tessellation-evaluation shader (TES) stage.
        /// </summary>
        internal int MaxTesTextureImageUnits { get; private set; }

        /// <summary>
        /// Gets the maximum number of texture image units supported by the fragment/pixel shader stage.
        /// </summary>
        internal int MaxFragmentTextureImageUnits { get; private set; }
    }
}
