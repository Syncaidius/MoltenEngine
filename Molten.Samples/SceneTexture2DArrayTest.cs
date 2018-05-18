using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SceneTexture2DArrayTest : SampleSceneGame
    {
        public override string Description => "A simple test of texture arrays via a material shared between two parented objects.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<CubeArrayVertex> _mesh;

        public SceneTexture2DArrayTest(EngineSettings settings = null) : base("2D Texture Array", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            _mesh.SetVertices(SampleVertexData.TextureArrayCubeVertices);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTextureArray2D.sbm");
            cr.Load<TextureData>("128_1.dds");
            cr.Load<TextureData>("128_2.dds");
            cr.Load<TextureData>("128_3.dds");
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
            TextureData texData = cr.Get<TextureData>(cr[1]);
            ITexture2D texture = null;

            if (texData != null)
            {
                texture = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
                {
                    Width = texData.Width,
                    Height = texData.Height,
                    MipMapLevels = texData.MipMapCount,
                    ArraySize = 3,
                    Flags = texData.Flags,
                    Format = texData.Format,
                });
                texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 0);
            }

            texData = cr.Get<TextureData>(2);
            if (texData != null)
                texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 1);

            texData = cr.Get<TextureData>(3);
            if (texture != null)
                texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 2);

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
