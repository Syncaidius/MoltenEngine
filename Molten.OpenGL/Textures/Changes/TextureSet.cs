using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    internal class TextureSet<T> : ITextureChange where T: struct
    {
        public int MipLevel;
        public T[] Data;
        public int StartIndex;
        public int Pitch;
        public int ArrayIndex;

        public int Count;
        public int Stride;
        public Rectangle? Area;

        public void Process(TextureBaseGL texture)
        {
            GL.BindTexture(texture.Target, texture.GLID);
            EngineInterop.PinObject(Data, ptr =>
            {
                // TODO revisit PixelFormat. This must correctly match the texture's internal format. Also block-compressed textures.
                int levelWidth = texture.Width >> MipLevel;
                int levelHeight = texture.Height >> MipLevel;
                GL.TexSubImage2D(texture.Target, MipLevel, 0, 0, levelWidth, levelHeight, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            });

            texture.Device.Profiler.Current.UpdateSubresourceCount++;
        }
    }
}
