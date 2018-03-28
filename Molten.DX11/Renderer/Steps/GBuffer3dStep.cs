using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBuffer3dStep : DeferredRenderStep
    {
        internal RenderSurface Scene;
        internal RenderSurface Normals;
        internal RenderSurface Specular;
        internal RenderSurface Emissive;
        internal DepthSurface Depth;

        Material _matStandard;
        Material _matSansNormalMap;
        IShaderValue _texDiffuse;
        IShaderValue _texNormal;
        IShaderValue _texEmissive;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);

            string source = null;
            string namepace = "Molten.Graphics.Assets.gbuffer.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = renderer.ShaderCompiler.Compile(source, namepace);
                _matStandard = result["material", "gbuffer"] as Material;
                _matSansNormalMap = result["material", "gbuffer-sans-nmap"] as Material;
            }
        }

        internal override void UpdateSurfaces(RendererDX11 renderer,int width, int height)
        {
            // Dispose of current surfaces
            DisposeSurfaces();

            Scene = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Normals = new RenderSurface(renderer.Device, width, height, Format.R11G11B10_Float);
            Specular = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Emissive = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Depth = new DepthSurface(renderer.Device, width, height, DepthFormat.R24G8_Typeless);
        }

        private void DisposeSurfaces()
        {
            Scene?.Dispose();
            Normals?.Dispose();
            Specular?.Dispose();
            Emissive?.Dispose();
        }

        public override void Dispose()
        {
            DisposeSurfaces();
        }

        protected override void OnRender(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time)
        {
            // Clear surfaces
            Scene.Clear(renderer.Device, scene.BackgroundColor);
            Normals.Clear(renderer.Device, Color.White * 0.5f);
            Specular.Clear(renderer.Device, Color.Black);
            Emissive.Clear(renderer.Device, Color.Black);


        }
    }
}
