using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SceneTexture1DArrayTest : SampleSceneGame
    {
        public override string Description => "A sample of 1D texture arrays via a material shared between two parented objects.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<CubeArrayVertex> _mesh;

        public SceneTexture1DArrayTest(EngineSettings settings = null) : base("1D Texture Array", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            _mesh.SetVertices(SampleVertexData.TextureArrayCubeVertices);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTextureArray1D.mfx");
            cr.Load<TextureData>("1d_1.png");
            cr.Load<TextureData>("1d_2.png");
            cr.Load<TextureData>("1d_3.png");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            IMaterial mat = cr.Get<IMaterial>(0);

            if (mat == null)
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            TextureData texData = cr.Get<TextureData>(1);
            ITexture texture = Engine.Renderer.Resources.CreateTexture1D(new Texture1DProperties()
            {
                Width = texData.Width,
                MipMapLevels = texData.MipMapLevels,
                ArraySize = 3,
                Flags = texData.Flags,
                Format = texData.Format,
            });
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 0);

            texData = cr.Get<TextureData>(2);
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 1);

            texData = cr.Get<TextureData>(3);
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 2);

            mat.SetDefaultResource(texture, 0);
            _mesh.Material = mat;
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
