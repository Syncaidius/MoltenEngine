using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A helper class for packing multiple textures into a single texture atlas. 
    /// </summary>
    public class TexturePacker
    {
        /// <summary>
        /// A class which represents the location of a texture within the texture sheet.
        /// </summary>
        public class AtlasLocation
        {
            /// <summary>
            /// The bounds representing the position and size of a texture located on one of the atlas' array slices.
            /// </summary>
            public Rectangle Location;

            /// <summary>
            /// The atlas array slice that a texture is allocated.
            /// </summary>
            public int ArraySliceIndex;
        }

        public class Result
        {
            public Dictionary<ITexture2D, AtlasLocation> Locations;

            public ITexture2D AtlasTexture;
        }

        Dictionary<ITexture2D, AtlasLocation> _entries;
        Dictionary<BinPacker, HashSet<ITexture2D>> _texturesByPacker;
        GraphicsFormat _expectedFormat;
        List<BinPacker> _packers;
        int _width;
        int _height;
        int _maxSlices;

        /// <summary>
        /// Creates a new <see cref="TexturePacker"/> instance of the specified width and height, expecting the specified <see cref="GraphicsFormat"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="maxArraySlices">The maximum number of array slices allowed in the final texture atlas.</param>
        /// <param name="expectedFormat"></param>
        public TexturePacker(int width, int height, GraphicsFormat expectedFormat, int maxArraySlices = 1)
        {
            _width = width;
            _height = height;
            _maxSlices = maxArraySlices;
            _expectedFormat = expectedFormat;
            _packers = new List<BinPacker>();
            _texturesByPacker = new Dictionary<BinPacker, HashSet<ITexture2D>>();
            _entries = new Dictionary<ITexture2D, AtlasLocation>();
            AddPacker();
        }

        private BinPacker AddPacker()
        {
            BinPacker packer = new BinPacker(_width, _height);
            _packers.Add(packer);
            _texturesByPacker.Add(packer, new HashSet<ITexture2D>());

            return packer;
        }

        /// <summary>
        /// Adds a <see cref="ITexture2D"/> to the current <see cref="TexturePacker"/> instance. 
        /// Returns null if the texture cannot fit on the atlas or it has reached the maximum number of array slices for the current device.
        /// </summary>
        /// <param name="texture">The texture to add.</param>
        /// <returns></returns>
        public AtlasLocation Add(ITexture2D texture)
        {
            if (texture.Format != _expectedFormat)
                throw new GraphicsFormatException(_expectedFormat);

            if (texture.Width > _width || texture.Height > _height)
                throw new Exception($"The provided texture {texture.Width}x{texture.Height} is too large. Maximum size is {_width}x{_height}");

            Rectangle? rect = null;
            AtlasLocation result = new AtlasLocation();


            for(int i = 0; i < _packers.Count; i++)
            {
                rect = _packers[i].Insert(texture.Width, texture.Height);
                if (rect != null)
                    break;
            }
            
            // Did we find a spot for the texture on an existing atlas slice?
            // If not, create a new array slice and add the texture to it.
            if(rect == null)
            {
                BinPacker packer = AddPacker();
                rect = packer.Insert(texture.Width, texture.Height);
            }
            
            return result;
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
            _packers.Clear();
            _texturesByPacker.Clear();

            // Restore the first packer. There is always at least one.
            AddPacker();
        }

        /// <summary>
        /// Packs the provided list of textures into a single texture.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="textures"></param>
        /// <param name="atlasFlags"></param>
        /// <returns></returns>
        public static Result Pack(IRenderer renderer, IList<ITexture2D> textures, TextureFlags atlasFlags = TextureFlags.None)
        {
            if (textures == null)
                throw new NullReferenceException("The provided texture list was null.");

            if (textures.Count == 0)
                throw new Exception("The provided texture list was empty.");

            int width = 0;
            int height = 0;
            HashSet<GraphicsFormat> formats = new HashSet<GraphicsFormat>();

            // Find out what dimensions we need to correctly fit all textures
            foreach (ITexture2D tex in textures)
            {
                width = Math.Max(width, tex.Width);
                height = Math.Max(height, tex.Height);
                formats.Add(tex.Format);
            }

            if (formats.Count > 1)
                throw new GraphicsFormatException(formats.ToList());

            TexturePacker packer = new TexturePacker(width, height, textures[0].Format);
            foreach (ITexture2D tex in textures)
                packer.Add(tex);

            return new Result()
            {
                Locations = new Dictionary<ITexture2D, AtlasLocation>(packer._entries),
                AtlasTexture = packer.Build(),
            };
        }
    }
}
