using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace Molten.Graphics
{
    public class Texture2DGL : TextureBaseGL, ITexture2D
    {
        public Texture2DGL(Texture2DGL descTexture, TextureFlags flags) : 
            this(descTexture.Renderer as RendererGL, descTexture.Width, descTexture.Height, descTexture.Format, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags, descTexture.SampleCount)
        {
        }

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal Texture2DGL(Texture2DGL descTexture)
            : this(descTexture.Renderer as RendererGL, descTexture.Width, descTexture.Height, descTexture.Format, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags, descTexture.SampleCount)
        { }

        internal Texture2DGL(
            RendererGL renderer,
            int width,
            int height,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
            int mipCount = 1,
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None,
            int sampleCount = 1)
            : base(renderer, width, height, 1, TextureTarget.Texture2d, mipCount, arraySize, sampleCount, format, flags)
        {

        }

        public Texture2DProperties Get2DProperties()
        {
            return new Texture2DProperties()
            {
                Width = Width,
                Height = Height,
                ArraySize = ArraySize,
                Flags = Flags,
                Format = Format,
                MipMapLevels = MipMapCount,
                SampleCount = SampleCount,
            };
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount, int newArraySize, GraphicsFormat newFormat)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight)
        {
            throw new NotImplementedException();
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
                Gl.TexImage2D(target, i, Format.ToInternal(), Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }
    }
}
