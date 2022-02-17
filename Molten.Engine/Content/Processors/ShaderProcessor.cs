using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Molten.Content
{
    public class ShaderProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(IShader) };

        public override Type[] RequiredServices { get; } = { typeof(RenderService) };

        public override void OnRead(ContentContext context)
        {
            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 2048, true))
                {
                    string source = reader.ReadToEnd();
                    ShaderCompileResult r = context.Engine.Renderer.Resources.CompileShaders(ref source, context.Filename);
                    foreach (ShaderClassType classType in r.ShaderGroups.Keys)
                    {
                        List<IShaderElement> list = r.ShaderGroups[classType];
                        foreach (IShader shader in list)
                        {
                            if (shader is IMaterial mat)
                                context.AddOutput(mat);
                            else if (shader is IComputeTask ct)
                                context.AddOutput(ct);

                            context.AddOutput(shader);
                        }
                    }
                }
            }
        }

        public override void OnWrite(ContentContext context)
        {
            throw new NotImplementedException();
        }

        public override object OnGet(Engine engine, Type t, Dictionary<string, string> metadata, IList<object> groupContent)
        {
            string materialName = "";
            if (metadata.TryGetValue("name", out materialName))
            {
                foreach (object obj in groupContent)
                {
                    IMaterial mat = obj as IMaterial;
                    if (mat.Name == materialName)
                        return mat;
                }
            }

            return groupContent[0];
        }
    }
}
