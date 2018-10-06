using Molten.Graphics;
using Molten.Graphics.Textures;
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
            TextureData finalData = null;
            TextureReader texReader = null;
            bool isArray = false;
            int arrayCount = 1;
            string fn = context.Filename;
            bool compress = true;
            DDSFormat compressFormat = DDSFormat.DXT5;

            if (context.Metadata.TryGetValue("array", out string strIsArray))
                bool.TryParse(strIsArray, out isArray);

            if (context.Metadata.TryGetValue("count", out string strCount))
                int.TryParse(strCount, out arrayCount);

            if (context.Metadata.TryGetValue("compress", out string strCompress))
                bool.TryParse(strCompress, out compress);

            if (context.Metadata.TryGetValue("cformat", out string strCompressFormat))
                Enum.TryParse(strCompressFormat, out compressFormat);

            if (isArray)
            {
                fn = context.File.Name.Replace(context.File.Extension, "");
                fn = context.Filename.Replace(context.File.Name, $"{fn}_{{0}}{context.File.Extension}");
            }

            for (int i = 0; i < arrayCount; i++)
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

                finalData = finalData ?? new TextureData()
                {
                    Width = data.Width,
                    Height = data.Height,
                    MipMapLevels = data.MipMapLevels,
                    Format = data.Format,
                    ArraySize = arrayCount,
                    IsCompressed = data.IsCompressed,
                };
                finalData.Set(data, i);
            }

            // Compress or decompress
            if (compress)
            {
                // Don't block-compress 1D textures.
                if (finalData.Height > 1)
                {
                    if (!finalData.IsCompressed)
                        finalData.Compress(compressFormat);
                }
            }
            else
            {
                if (finalData.IsCompressed)
                    finalData.Decompress();
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

                        for (int i = 0; i < finalData.Levels.Length; i++)
                            existingData.Set(finalData, 0);
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
                context.Log.WriteError($"Unsupported texture type {context.ContentType}", context.Filename);

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

        public override void OnWrite(ContentContext context)
        {
            using (FileStream stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
            {
                string extension = context.File.Extension.ToLower();
                TextureWriter texWriter = null;

                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    switch (extension)
                    {
                        case ".dds":
                            texWriter = new DDSWriter(DDSFormat.DXT5);
                            break;

                        case ".png":
                            texWriter = new PNGWriter();
                            break;
                    }
                }

                if (texWriter == null)
                {
                    context.Log.WriteError($"Unable to write texture to file. Unsupported format: {extension}", context.Filename);
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
                            texWriter.WriteData(stream, dataToSave, context.Log);
                        }
                    }
                }
                else
                {
                    context.Log.WriteError("Directly writing is currently not supported. Try writing TextureData instead.");
                    // TODO finish support for writing textures directly

                    //ITexture tex = null;
                    //if (context.Input.TryGetValue(context.ContentType, out List<object> texturesToSave))
                    //{
                    //    if (texturesToSave.Count > 0)
                    //        tex = texturesToSave[0] as ITexture;

                    //    ITexture staging = 
                    //    texWriter.WriteData(stream, tex.GetData()
                    //}
                }
            }
        }
    }
}
