using Molten.Graphics;
using System.Text;

namespace Molten.Content
{
    public class ShaderProcessor : ContentProcessor<ShaderParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(IShader) };

        public override Type[] RequiredServices { get; } = { typeof(RenderService) };

        protected override void OnRead(ContentContext context, ShaderParameters p)
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

        protected override void OnWrite(ContentContext context, ShaderParameters p)
        {
            throw new NotImplementedException();
        }

        protected override object OnGet(Engine engine, Type contentType, ShaderParameters p, IList<object> groupContent)
        {
            if (!string.IsNullOrEmpty(p.MaterialName))
            {
                foreach (object obj in groupContent)
                {
                    IMaterial mat = obj as IMaterial;
                    if (mat.Name == p.MaterialName)
                        return mat;
                }
            }

            return groupContent[0];
        }
    }
}
