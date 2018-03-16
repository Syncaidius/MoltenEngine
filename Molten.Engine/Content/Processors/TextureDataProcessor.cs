using Molten.Graphics;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Content
{
    public class TexturedataProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; protected set; } = new Type[] { typeof(TextureData)};

        public override void OnRead(Engine engine, Logger log, Type contentType, Stream stream, Dictionary<string,string> metadata, FileInfo file, ContentResult output)
        {
            string extension = file.Extension.ToLower();
            TextureData data = null;
            TextureReader texReader = null;

            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                switch (extension)
                {
                    case ".dds":
                        texReader = new DDSReader(false);
                        break;

                    default:
                        texReader = engine.Renderer.Resources.GetDefaultTextureReader(file);
                        break;
                }

                if (texReader == null)
                    return;

                texReader.Read(reader);

                //output error, if one occurred.
                string error = texReader.Error;
                if (error != null)
                {
                    log.WriteError(error, file.ToString());
                    return;
                }
                else
                {
                    data = texReader.GetData();
                }

                //output error, if one occurred.
                error = texReader.Error;
                if (error != null)
                {
                    log.WriteError(error, file.ToString());
                }
                else
                {
                    if (data.MipMapCount == 1)
                    {
                        string genMipsVal = "";
                        if (metadata.TryGetValue("mipmaps", out genMipsVal))
                        {
                            if (genMipsVal == "true")
                            {
                                //if (!data.GenerateMipMaps())
                                 //   log.WriteError("[CONTENT] Unable to generate mip-maps for non-power-of-two texture.", file.ToString());
                            }
                        }
                    }

                    output.AddResult(contentType, data);

                    texReader.Dispose();
                }
            }
        }

        public override void OnWrite(Engine engine, Logger log, Type t, Stream stream, FileInfo file)
        {
            throw new NotImplementedException();
        }
    }
}
