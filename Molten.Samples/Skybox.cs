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
    public class SkyboxSample : SampleSceneGame
    {
        public override string Description => "A skybox demonstration.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<CubeArrayVertex> _mesh;

        public SkyboxSample(EngineSettings settings = null) : base("Skybox", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            _mesh.SetVertices(SampleVertexData.TextureArrayCubeVertices);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_dxt5.dds");
            cr.Load<ITextureCube>("cubemap.dds");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            IMaterial mat = cr.Get<IMaterial>("BasicTexture.mfx");

            if (mat == null)
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            ITexture2D texture = cr.Get<ITexture2D>("dds_dxt5.dds");
            MainScene.SkyboxTeture = cr.Get<ITextureCube>("cubemap.dds");

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
