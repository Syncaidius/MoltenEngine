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

        protected override bool OnRead(ContentHandle handle, TextureParameters parameters, object existingAsset, out object asset)
        {
            asset = null;
            string extension = handle.Info.Extension.ToLower();
            TextureData finalData = null;
            TextureReader texReader = null;
            string fn = handle.Path;

            if (parameters.ArraySize > 1)
            {
                fn = handle.Info.Name.Replace(handle.Info.Extension, "");
                fn = handle.RelativePath.Replace(handle.Info.Name, $"{fn}_{{0}}{handle.Info.Extension}");
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

                        data = texReader.Read(reader, handle.Manager.Log, handle.RelativePath);
                        texReader.Dispose();
                    }
                }

                // Load failed?
                if (data == null)
                    return false;

                if (data.MipMapLevels == 1)
                {
                    if (parameters.GenerateMipmaps)
                    {
                            //if (!data.GenerateMipMaps())
                            //   log.WriteError("[CONTENT] Unable to generate mip-maps for non-power-of-two texture.", file.ToString());
                    }
                }

                uint arraySize = typeof(ITextureCube).IsAssignableFrom(handle.ContentType) ? 6U : parameters.ArraySize;
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
                        finalData.Compress(parameters.BlockCompressionFormat.Value, handle.Manager.Log);
                }
            }

            // TODO improve for texture arrays - Only update the array slice(s) that have changed.
            // Check if an existing texture was passed in.
            if (handle.ContentType == typeof(TextureData))
            {
                asset = finalData;
                return true;
            }
            else
            {
                ITexture tex = existingAsset as ITexture;

                if (tex != null)
                    ReloadTexture(tex, finalData);
                else
                    CreateTexture(handle, finalData);
            }

            return true;
        }

        private ITexture CreateTexture(ContentHandle handle, TextureData data)
        {
            ITexture tex = null;
            ContentManager manager = handle.Manager;

            if (handle.ContentType == typeof(ITexture2D))
                tex = manager.Engine.Renderer.Resources.CreateTexture2D(data);
            else if (handle.ContentType == typeof(ITextureCube))
                tex = manager.Engine.Renderer.Resources.CreateTextureCube(data);
            else if (handle.ContentType == typeof(ITexture))
                tex = manager.Engine.Renderer.Resources.CreateTexture1D(data);
            else
                manager.Log.Error($"Unsupported texture type {handle.ContentType}", handle.RelativePath);

            return tex;
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

        protected override bool OnWrite(ContentHandle handle, TextureParameters parameters, object asset)
        {
            using (FileStream stream = new FileStream(handle.Path, FileMode.Create, FileAccess.Write))
            {
                string extension = handle.Info.Extension.ToLower();
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
                    handle.Manager.Log.Error($"Unable to write texture to file. Unsupported format: {extension}", handle.Path);
                    return false;
                }

                // TODO improve for texture arrays - Only update the array slice(s) that have changed.
                // Check if an existing texture was passed in.
                if (handle.ContentType == typeof(TextureData))
                {
                    TextureData dataToSave = asset as TextureData;
                    texWriter.WriteData(stream, dataToSave, handle.Manager.Log, handle.Path);
                }
                else
                {
                    // TODO finish support for writing textures directly

                    ITexture tex = asset as ITexture;
                    ITexture staging = null;

                    switch (tex)
                    {
                        case ITextureCube texCube:
                            staging = handle.Manager.Engine.Renderer.Resources.CreateTextureCube(new Texture2DProperties()
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
                            staging = handle.Manager.Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
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
                            staging = handle.Manager.Engine.Renderer.Resources.CreateTexture1D(new Texture1DProperties()
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

                        texWriter.WriteData(stream, tData, handle.Manager.Log, handle.RelativePath);
                    }
                }

                return true;
            }
        }
    }
}
