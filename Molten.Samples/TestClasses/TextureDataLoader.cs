using Molten.Graphics;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    /// <summary>A Type processor for loading/saving pixel shader effects.</summary>
    internal class TextureDataLoader
    {
        public TextureDataLoader()
        {

        }

        public TextureData Read(Logger log, Engine engine, BinaryReader reader, string filename, bool allowCompressed = true)
        {
            FileInfo file = new FileInfo(filename);
            string extension = file.Extension.ToLower();
            TextureData asset = null;
            TextureReader texReader = null;

            //select an appropriate texture reader.
            if (extension == ".dds")
            {
                texReader = new DDSReader(allowCompressed);
                texReader.Read(reader);
            }
            //else
            //    texReader = new DefaultTextureReader(client.GraphicsDevice, reader);

            //make sure a reader is available to use.
            if (texReader == null)
            {
                log.WriteError(filename + ": Unsupported texture file format.");
                asset = new TextureData()
                {
                    ArraySize = 1,
                    Width = 1,
                    Height = 1,
                    Format = GraphicsFormat.R8G8B8A8_UNorm,
                    Flags = TextureFlags.None,
                    Levels = new TextureData.Slice[0],
                    MipMapCount = 1,
                };
            }
            else
            {
                //output error, if one occurred.
                string error = texReader.Error;

                if (error != null)
                    log.WriteError(filename + ": " + error);
                else
                    asset = texReader.GetData(); //retrieve texture in whatever state it succeeded to be created.

                texReader.Dispose();
            }

            return asset;
        }
    }
}
