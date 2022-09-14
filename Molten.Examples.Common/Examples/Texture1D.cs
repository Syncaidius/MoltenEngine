using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Texture 1D", "Demonstrates how 1D textures are used")]
    public class Texture1DCube : MoltenExample
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture1D.mfx");
            _hTexture = loader.Load<ITexture>("assets/1d_1.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }
    }
}
