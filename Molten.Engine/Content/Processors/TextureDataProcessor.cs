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

        public override void OnRead(ContentContext context)
        {
            string extension = context.File.Extension.ToLower();
            TextureData data = null;
            TextureReader texReader = null;

            using (BinaryReader reader = new BinaryReader(context.Stream, Encoding.UTF8, true))
            {
                switch (extension)
                {
                    case ".dds":
                        texReader = new DDSReader(false);
                        break;

                    default:
                        texReader = context.Engine.Renderer.Resources.GetDefaultTextureReader(context.File);
                        break;
                }

                if (texReader == null)
                    return;

                texReader.Read(reader);

                //output error, if one occurred.
                string error = texReader.Error;
                if (error != null)
                {
                    context.Log.WriteError(error, context.Filename);
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
                    context.Log.WriteError(error, context.Filename);
                }
                else
                {
                    if (data.MipMapCount == 1)
                    {
                        string genMipsVal = "";
                        if (context.Metadata.TryGetValue("mipmaps", out genMipsVal))
                        {
                            if (genMipsVal == "true")
                            {
                                //if (!data.GenerateMipMaps())
                                 //   log.WriteError("[CONTENT] Unable to generate mip-maps for non-power-of-two texture.", file.ToString());
                            }
                        }
                    }

                    context.AddOutput(context.ContentType, data);
                    texReader.Dispose();
                }
            }
        }

        public override void OnWrite(ContentContext context)
        {
            throw new NotImplementedException();
        }
    }
}
