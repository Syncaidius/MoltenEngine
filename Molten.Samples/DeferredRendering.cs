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
    public class DeferredRenderingSample : SampleSceneGame
    {
        public override string Description => "A test/sample for deferred rendering";

        SceneObject _parent;
        SceneObject _child;
        IMesh<GBufferVertex> _mesh;

        public DeferredRenderingSample(EngineSettings settings = null) : base("Deferred Rendering", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            SampleScene.RenderData.Flags |= SceneRenderFlags.Deferred;
            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("dds_test.dds");
            cr.Load<ITexture2D>("dds_test_n.dds");
            cr.Load<ITexture2D>("dds_test_e.dds");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = MeshHelper.Cube(engine.Renderer);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
            AcceptPlayerInput = false;
            Player.Transform.LocalPosition = new Vector3F(0, 0, -8);
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            ITexture2D diffuseMap = content.Get<ITexture2D>(cr.RequestedFiles[0]);
            _mesh.SetResource(diffuseMap, 0);

            ITexture2D normalMap = content.Get<ITexture2D>(cr.RequestedFiles[1]);
            _mesh.SetResource(normalMap, 1);

            ITexture2D emssiveMap = content.Get<ITexture2D>(cr.RequestedFiles[2]);
            _mesh.SetResource(emssiveMap, 2);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
