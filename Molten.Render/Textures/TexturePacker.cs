using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A helper class for packing multiple textures into a single texture. 
    /// </summary>
    public class TexturePacker
    {
        BinPacker _packer;
        Dictionary<ITexture2D, Rectangle> _entries;
        GraphicsFormat _expectedFormat;

        /// <summary>
        /// Creates a new <see cref="TexturePacker"/> instance of the specified width and height, expecting the specified <see cref="GraphicsFormat"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="expectedFormat"></param>
        public TexturePacker(int width, int height, GraphicsFormat expectedFormat)
        {
            _expectedFormat = expectedFormat;
            _packer = new BinPacker(width, height);
            _entries = new Dictionary<ITexture2D, Rectangle>();
        }

        /// <summary>
        /// Adds a <see cref="ITexture2D"/> to the current <see cref="TexturePacker"/> instance.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Rectangle? Add(ITexture2D texture)
        {
            if (texture.Format != _expectedFormat)
                throw new GraphicsFormatException(_expectedFormat);

            Rectangle? result = _packer.Insert(texture.Width, texture.Height);

            // TODO Implement and return a TexturePackerLocation value. Contains a rectangle and an array slice index.
            // TODO If result is null, check if we can add a new page to the packer. Try again if true.

            return result;
        }

        public void Remove(ITexture2D texture)
        {
            throw new NotImplementedException();
              // TODO Remove from packer.
        }

        /// <summary>
        /// Builds an atlas texture containing all of the textures that were added to the current <see cref="TexturePacker"/> instance.
        /// </summary>
        /// <returns></returns>
        public ITexture2D Build()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            // TODO clear all packers held in current TexturePacker instance.
            _packer.Clear();
        }

        /// <summary>
        /// Generates a 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="textures"></param>
        /// <param name="atlasFlags"></param>
        /// <returns></returns>
        public ITexture2D Generate(IRenderer renderer, IList<ITexture2D> textures, TextureFlags atlasFlags = TextureFlags.None)
        {
            throw new NotImplementedException();
            // TODO Re-implement code below into the instantiated version of this class.


            //HashSet<GraphicsFormat> _formats = new HashSet<GraphicsFormat>();
            //foreach (ITexture2D tex in textures)
            //    _formats.Add(tex.Format);

            //if (_formats.Count > 1)
            //    throw new GraphicsFormatException(_formats.ToArray());

            //int sizeW = 128;
            //int sizeH = 128;
            //BinPacker packer = new BinPacker(sizeW, sizeH);
            //bool tooSmall = false;

            //while (true)
            //{
            //    foreach (ITexture2D tex in textures)
            //    {
            //        if (packer.Insert(tex.Width, tex.Height) == null)
            //        {
            //            break;
            //        }
            //    }

            //    // Do we need to resize the packer and start over?
            //    if (tooSmall)
            //    {
            //        // Resize 1 dimension at a time.
            //        if (sizeH < sizeW)
            //            sizeH *= 2;
            //        else
            //            sizeW *= 2;

            //        packer = new BinPacker(sizeW, sizeH);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}

            //TexturePacker atlas = new TexturePacker(renderer, sizeW, sizeH, packer, textures);

            //// TODO ensure all textures are of the same/compatible formats. List all detected formats in an error if not.
            //// TODO create new texture atlas and add all the provided textures.
            //// TODO bin pack all provided textures before adding to sheet.
            //// Log warnings for any textures that are not named. Perhaps don't add them.

            //return atlas;
        }
    }
}
