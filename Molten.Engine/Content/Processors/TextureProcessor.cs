using Molten.Graphics;
using Molten.Graphics.Textures;
using Molten.Graphics.Textures.DDS;
using System.Text;

namespace Molten.Content
{
    public class TextureProcessor : ContentProcessor<TextureParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(ITexture), typeof(TextureData) };

        public override Type[] RequiredServices { get; } = { typeof(RenderService) };

        protected override void OnRead(ContentContext context, TextureParameters parameters)
        {
            string extension = context.File.Extension.ToLower();
            TextureData finalData = null;
            TextureReader texReader = null;
            string fn = context.Filename;

            if (parameters.ArraySize > 1)
            {
                fn = context.File.Name.Replace(context.File.Extension, "");
                fn = context.Filename.Replace(context.File.Name, $"{fn}_{{0}}{context.File.Extension}");
            }

            for (uint i = 0; i < parameters.ArraySize; i++)
            {
                string finalFn = string.Format(fn, i + 1);

                if (!File.Exists(finalFn))
                    continue;

                TextureData data = null;
                using (Stream stream = new FileStream(finalFn, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                    {
                        switch (extension)
                        {
                            case ".dds":
                                texReader = new DDSReader();
                                break;

                            // Although the default texture reader can load all of the formats that Magick supports, we'll stick to ones we fully support for now.
                            // Formats such as .gif can be handled as texture arrays later down the line.
                            case ".png":
                            case ".jpeg":
                            case ".bmp":
                                texReader = new DefaultTextureReader();
                                break;

                            default:
                                texReader = null;
                                break;
                        }

                        data = texReader.Read(reader, context.Log, context.Filename);
                        texReader.Dispose();
                    }
                }

                // Load failed?
                if (data == null)
                    return;

                if (data.MipMapLevels == 1)
                {
                    if (parameters.GenerateMipmaps)
                    {
                            //if (!data.GenerateMipMaps())
                            //   log.WriteError("[CONTENT] Unable to generate mip-maps for non-power-of-two texture.", file.ToString());
                    }
                }

                uint arraySize = typeof(ITextureCube).IsAssignableFrom(context.ContentType) ? 6U : parameters.ArraySize;
                finalData = finalData ?? new TextureData(data.Width, data.Height, data.MipMapLevels, arraySize)
                {
                    Format = data.Format,
                    IsCompressed = data.IsCompressed,
                };

                finalData.Set(data, i);
            }

            // Compress or decompress
            if (parameters.BlockCompressionFormat.HasValue)
            {
                // Don't block-compress 1D textures.
                if (finalData.Height > 1)
                {
                    if (!finalData.IsCompressed)
                        finalData.Compress(parameters.BlockCompressionFormat.Value, context.Log);
                }
            }

            // TODO improve for texture arrays - Only update the array slice(s) that have changed.
            // Check if an existing texture was passed in.
            if (context.ContentType == typeof(TextureData))
            {
                if (context.Input.TryGetValue(context.ContentType, out List<object> existingObjects))
                {
                    if (existingObjects.Count > 0)
                    {
                        TextureData existingData = existingObjects[0] as TextureData;
                        existingData.Set(finalData);
                    }
                }
                else
                {
                    context.AddOutput(finalData);
                }
                return;
            }
            else
            {
                ITexture tex = null;
                if (context.Input.TryGetValue(context.ContentType, out List<object> existingObjects))
                {
                    if (existingObjects.Count > 0)
                        tex = existingObjects[0] as ITexture;
                }

                if (tex != null)
                    ReloadTexture(tex, finalData);
                else
                    CreateTexture(context, finalData);
            }
        }

        private void CreateTexture(ContentContext context, TextureData data)
        {
            ITexture tex = null;

            if (context.ContentType == typeof(ITexture2D))
                tex = context.Engine.Renderer.Resources.CreateTexture2D(data);
            else if (context.ContentType == typeof(ITextureCube))
                tex = context.Engine.Renderer.Resources.CreateTextureCube(data);
            else if (context.ContentType == typeof(ITexture))
                tex = context.Engine.Renderer.Resources.CreateTexture1D(data);
            else
                context.Log.Error($"Unsupported texture type {context.ContentType}", context.Filename);

            if (tex != null)
                context.AddOutput(context.ContentType, tex);
        }

        private void ReloadTexture(ITexture tex, TextureData data)
        {
            switch (tex)
            {
                case ITextureCube texCube:
                    // TODO include mip-map count in resize
                    if (texCube.Width != data.Width ||
                        texCube.Height != data.Height ||
                        tex.MipMapCount != data.MipMapLevels)
                        texCube.Resize(data.Width, data.Height, data.MipMapLevels);

                    texCube.SetData(data, 0, 0, data.MipMapLevels, Math.Min(data.ArraySize, 6), 0, 0);
                    break;

                case ITexture2D tex2d:
                    // TODO include mip-map count in resize
                    if (tex2d.Width != data.Width ||
                        tex2d.Height != data.Height ||
                        tex2d.ArraySize != data.ArraySize ||
                        tex.MipMapCount != data.MipMapLevels)
                    {
                        tex2d.Resize(data.Width, data.Height, data.MipMapLevels, data.ArraySize, data.Format);
                    }

                    tex2d.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize, 0, 0);
                    break;

                default:
                    // TODO include mip-map count in resize
                    if (tex.Width != data.Width || tex.MipMapCount != data.MipMapLevels)
                        tex.Resize(data.Width, data.MipMapLevels, data.Format);

                    tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize, 0, 0);
                    break;
            }
        }

        protected override void OnWrite(ContentContext context, TextureParameters parameters)
        {
            using (FileStream stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
            {
                string extension = context.File.Extension.ToLower();
                TextureWriter texWriter = null;

                switch (extension)
                {
                    case ".dds":
                        DDSFormat? pFormat = parameters.BlockCompressionFormat;
                        DDSFormat ddsFormat = pFormat.HasValue ? pFormat.Value : DDSFormat.DXT5;
                        texWriter = new DDSWriter(ddsFormat);
                        break;

                    case ".png":
                        texWriter = new PNGWriter();
                        break;

                    case ".jpeg":
                    case ".jpg":
                        texWriter = new JPEGWriter();
                        break;

                    case ".bmp":
                        texWriter = new BMPWriter();
                        break;
                }

                if (texWriter == null)
                {
                    context.Log.Error($"Unable to write texture to file. Unsupported format: {extension}", context.Filename);
                    return;
                }

                // TODO improve for texture arrays - Only update the array slice(s) that have changed.
                // Check if an existing texture was passed in.
                if (context.ContentType == typeof(TextureData))
                {
                    if (context.Input.TryGetValue(context.ContentType, out List<object> objectsToSave))
                    {
                        if (objectsToSave.Count > 0)
                        {
                            TextureData dataToSave = objectsToSave[0] as TextureData;
                            texWriter.WriteData(stream, dataToSave, context.Log, context.Filename);
                        }
                    }
                }
                else
                {
                    // TODO finish support for writing textures directly

                    ITexture tex = null;
                    if (context.Input.TryGetValue(context.ContentType, out List<object> texturesToSave))
                    {
                        if (texturesToSave.Count > 0)
                            tex = texturesToSave[0] as ITexture;

                        ITexture staging = null;
                        switch (tex)
                        {
                            case ITextureCube texCube:
                                staging = context.Engine.Renderer.Resources.CreateTextureCube(new Texture2DProperties()
                                {
                                    Flags = TextureFlags.Staging,
                                    Format = texCube.DataFormat,
                                    ArraySize = texCube.ArraySize,
                                    Height = texCube.Height,
                                    MipMapLevels = texCube.MipMapCount,
                                    MultiSampleLevel = texCube.MultiSampleLevel,
                                    Width = texCube.Width,
                                });
                                break;

                            case ITexture2D tex2D:
                                staging = context.Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
                                {
                                    Flags = TextureFlags.Staging,
                                    Format = tex2D.DataFormat,
                                    ArraySize = tex2D.ArraySize,
                                    Height = tex2D.Height,
                                    MipMapLevels = tex2D.MipMapCount,
                                    MultiSampleLevel = tex2D.MultiSampleLevel,
                                    Width = tex2D.Width,
                                });
                                break;

                            case ITexture tex1D:
                                staging = context.Engine.Renderer.Resources.CreateTexture1D(new Texture1DProperties()
                                {
                                    Flags = TextureFlags.Staging,
                                    Format = tex1D.DataFormat,
                                    ArraySize = tex1D.ArraySize,
                                    MipMapLevels = tex1D.MipMapCount,
                                    Width = tex1D.Width,
                                });
                                break;
                        }

                        if (staging != null)
                        {
                            TextureData tData = null;
                            tex.GetData(staging, (data) =>
                            {
                                tData = data;
                            });

                            while (tData == null)
                                Thread.Sleep(5);

                            texWriter.WriteData(stream, tData, context.Log, context.Filename);
                        }
                    }
                }
            }
        }
    }
}
