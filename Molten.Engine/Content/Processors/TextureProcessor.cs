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
    public class TextureProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; protected set; } = new Type[] { typeof(ITexture), typeof(TextureData) };

        public override void OnRead(ContentContext context)
        {
            string extension = context.File.Extension.ToLower();
            TextureData data = null;
            TextureReader texReader = null;

            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
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

                    texReader.Read(reader);

                    // Output error, if one occurred.
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
                    texReader.Dispose();

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


                        if (context.ContentType == typeof(TextureData))
                        {
                            context.AddOutput(data);
                            return;
                        }

                        // Check if an existing texture was passed in.
                        ITexture tex = null;
                        if (context.Input.TryGetValue(context.ContentType, out List<object> existingObjects))
                        {
                            if (existingObjects.Count > 0)
                                tex = existingObjects[0] as ITexture;
                        }

                        if (tex != null)
                        {
                            OnReloadTexture(tex, data);
                        }
                        else
                        {
                            if (context.ContentType == typeof(ITexture2D))
                                tex = context.Engine.Renderer.Resources.CreateTexture2D(data);
                            else if (context.ContentType == typeof(ITextureCube))
                                tex = context.Engine.Renderer.Resources.CreateTextureCube(data);
                            else if (context.ContentType == typeof(ITexture))
                                tex = context.Engine.Renderer.Resources.CreateTexture1D(data);
                            else
                                context.Log.WriteError($"Unsupported texture type {context.ContentType}", context.Filename);

                            if (tex != null)
                                context.AddOutput(context.ContentType, tex);
                        }
                    }
                }
            }
        }

        private void OnReloadTexture(ITexture tex, TextureData data)
        {
            switch (tex)
            {
                case ITextureCube texCube:
                    // TODO include mip-map count in resize
                    if (texCube.Width != data.Width || 
                        texCube.Height != data.Height || 
                        tex.MipMapCount != data.MipMapCount)
                        texCube.Resize(data.Width, data.Height, data.MipMapCount);

                    texCube.SetData(data, 0, 0, data.MipMapCount, Math.Min(data.ArraySize, 6), 0, 0);
                    break;

                case ITexture2D tex2d:
                    // TODO include mip-map count in resize
                    if (tex2d.Width != data.Width ||
                        tex2d.Height != data.Height ||
                        tex2d.ArraySize != data.ArraySize ||
                        tex.MipMapCount != data.MipMapCount)
                    {
                        tex2d.Resize(data.Width, data.Height, data.MipMapCount, data.ArraySize, data.Format);
                    }

                    tex2d.SetData(data, 0, 0, data.MipMapCount, data.ArraySize, 0, 0);
                    break;

                default:
                    // TODO include mip-map count in resize
                    if (tex.Width != data.Width || tex.MipMapCount != data.MipMapCount)
                        tex.Resize(data.Width, data.MipMapCount, data.Format);

                    tex.SetData(data, 0, 0, data.MipMapCount, data.ArraySize, 0, 0);
                    break;
            }
        }

        public override void OnWrite(ContentContext context)
        {
            throw new NotImplementedException();
        }
    }
}
