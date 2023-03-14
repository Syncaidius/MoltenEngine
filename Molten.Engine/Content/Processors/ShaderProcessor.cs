using System.Text;
using Molten.Graphics;

namespace Molten.Content
{
    public class ShaderProcessor : ContentProcessor<ShaderParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(HlslShader) };

        public override Type[] RequiredServices { get; } = { typeof(RenderService) };

        public override Type PartType { get; } = typeof(HlslShader);

        protected override bool OnReadPart(ContentLoadHandle handle, Stream stream, ShaderParameters parameters, object existingPart, out object partAsset)
        {
            partAsset = null;

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 2048, true))
            {
                string source = reader.ReadToEnd();
                ShaderCompileResult result = handle.Manager.Engine.Renderer.Device.CompileShaders(ref source, handle.RelativePath);

                // Temp solution to limitation of new content manager.
                partAsset = result[0];

                /*foreach (IShader shader in list)
                {
                    if (shader is IMaterial mat)
                        context.AddOutput(mat);
                    else if (shader is IComputeTask ct)
                        context.AddOutput(ct);

                    context.AddOutput(shader);
                }*/
            }

            return true;
        }

        protected override bool OnBuildAsset(ContentLoadHandle handle, ContentLoadHandle[] parts, ShaderParameters parameters, object existingAsset, out object asset)
        {
            if (parts.Length > 1)
                handle.LogWarning($"ShaderProcessor does not support multi-part assets. Only first part will be used");

            asset = parts[0].Asset;
            return true;
        }

        protected override bool OnWrite(ContentHandle handle, Stream stream, ShaderParameters parameters, object asset)
        {
            throw new NotImplementedException();
        }
    }
}
