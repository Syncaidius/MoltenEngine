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
    public class MaterialProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; protected set; } = new Type[] { typeof(IShader)};

        public override void OnRead(Engine engine, Logger log, Type t, Stream stream, Dictionary<string,string> metadata, FileInfo file, ContentResult output)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 2048, true))
            {
                string source = reader.ReadToEnd();
                ShaderCompileResult r = engine.Renderer.Resources.CreateShaders(source, file.ToString());
                foreach(string group in r.ShaderGroups.Keys)
                {
                    List<IShader> list = r.ShaderGroups[group];
                    foreach (IShader shader in list)
                        output.AddResult(shader);
                }
            }
        }

        public override void OnWrite(Engine engine, Logger log, Type t, Stream stream, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public override object OnGet(Engine engine, Type t, Dictionary<string, string> metadata, List<object> groupContent)
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
