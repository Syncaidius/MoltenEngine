using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBuffer3dStep : RenderStepBase
    {
        Material _matStandard;
        Material _matSansNormalMap;

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

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height) { }

        public override void Dispose()
        {
            _matStandard.Dispose();
            _matSansNormalMap.Dispose();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            GraphicsDevice device = renderer.Device;
            switch (link.Previous.Step)
            {
                case StartStep start:
                    device.SetRenderSurface(start.Scene, 0);
                    device.SetRenderSurface(start.Normals, 1);
                    device.SetRenderSurface(start.Emissive, 2);
                    break;

                    // TODO add alternate HDR start step here (which should be used in conjunction HDR textures, HDR RTs and so on).
            }
            
            scene.Render3D(device, renderer);
        }
    }
}
