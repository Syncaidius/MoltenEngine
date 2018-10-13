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
    public class SceneParentingTest : SampleSceneGame
    {
        public override string Description => "A simple test of the scene object parenting system";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexColor> _mesh;

        public SceneParentingTest(EngineSettings settings = null) : base("Scene Parenting", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicColor.mfx");
            cr.OnCompleted += Cr_OnCompleted; ;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            _mesh.SetVertices(SampleVertexData.ColoredCube);

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

            _mesh.Material = mat;
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
