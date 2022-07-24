using Molten.Graphics;
using System.Text;

namespace Molten.Content
{
    public class ShaderProcessor : ContentProcessor<ShaderParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(IShader) };

        public override Type[] RequiredServices { get; } = { typeof(RenderService) };

        protected override bool OnRead(ContentHandle handle, ShaderParameters parameters, object existingAsset, out object asset)
        {
            asset = null;

            using (Stream stream = new FileStream(handle.RelativePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 2048, true))
                {
                    string source = reader.ReadToEnd();
                    ShaderCompileResult r = handle.Manager.Engine.Renderer.Resources.CompileShaders(ref source, handle.RelativePath);
                    foreach (ShaderClassType classType in r.ShaderGroups.Keys)
                    {
                        List<IShaderElement> list = r.ShaderGroups[classType];

                        // Temp solution to limitation of new content manager.
                        asset = list[0];
                        break;
                        /*foreach (IShader shader in list)
                        {
                            if (shader is IMaterial mat)
                                context.AddOutput(mat);
                            else if (shader is IComputeTask ct)
                                context.AddOutput(ct);

                            context.AddOutput(shader);
                        }*/
                    }
                }
            }

            return true;
        }


        protected override bool OnWrite(ContentHandle handle, ShaderParameters parameters, object asset)
        {
            throw new NotImplementedException();
        }
    }
}
