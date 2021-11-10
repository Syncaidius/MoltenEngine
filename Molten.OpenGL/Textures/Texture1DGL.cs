using OpenGL;
using System;

namespace Molten.Graphics
{
    public class Texture1DGL : TextureBaseGL, ITexture
    {
        public Texture1DGL(Texture1DGL descTexture, TextureFlags flags) :
            this(descTexture.Renderer as RendererGL, descTexture.Width, descTexture.Format, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags)
        {
        }

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal Texture1DGL(Texture1DGL descTexture)
            : this(descTexture.Renderer as RendererGL, descTexture.Width, descTexture.Format, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags)
        { }

        internal Texture1DGL(
            RendererGL renderer,
            int width,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
            int mipCount = 1,
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, 1, 1, TextureTarget.Texture1d, mipCount, arraySize, 1, format, flags)
        {

        }

        protected unsafe override void CreateResource(ref uint id, TextureTarget target, bool isResizing)
        {
            int mipLevels = MipMapCount - 1;
            int baseLevel = 0;

            id = Gl.GenTexture();
            Gl.BindTexture(target, id);
            Gl.TexParameterI(target, TextureParameterName.TextureBaseLevel, &baseLevel);
            Gl.TexParameterI(target, TextureParameterName.TextureMaxLevel, &mipLevels);

            // NOTE - Khronos.org: "Again, if you use more than one mipmaps, you should change the GL_TEXTURE_MAX_LEVEL to state how many you will use 
            //               (minus 1. The base/max level is a closed range), then perform a glTexImage2D (note the lack of "Sub") for each mipmap."
            for (int i = 0; i < MipMapCount; i++)
                Gl.TexImage1D(target, i, Format.ToInternal(), Width, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }
    }
}
