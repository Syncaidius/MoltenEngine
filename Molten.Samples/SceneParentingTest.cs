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

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted; ;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            VertexColor[] verts = new VertexColor[]{
                new VertexColor(new Vector3F(-1,-1,-1), Color.Red), //front
                new VertexColor(new Vector3F(-1,1,-1), Color.Red),
                new VertexColor(new Vector3F(1,1,-1), Color.Red),
                new VertexColor(new Vector3F(-1,-1,-1), Color.Red),
                new VertexColor(new Vector3F(1,1,-1), Color.Red),
                new VertexColor(new Vector3F(1,-1,-1), Color.Red),

                new VertexColor(new Vector3F(-1,-1,1), Color.Blue), //back
                new VertexColor(new Vector3F(1,1,1), Color.Blue),
                new VertexColor(new Vector3F(-1,1,1), Color.Blue),
                new VertexColor(new Vector3F(-1,-1,1),Color.Blue),
                new VertexColor(new Vector3F(1,-1,1), Color.Blue),
                new VertexColor(new Vector3F(1,1,1), Color.Blue),

                new VertexColor(new Vector3F(-1,1,-1), Color.Yellow), //top
                new VertexColor(new Vector3F(-1,1,1), Color.Yellow),
                new VertexColor(new Vector3F(1,1,1), Color.Yellow),
                new VertexColor(new Vector3F(-1,1,-1), Color.Yellow),
                new VertexColor(new Vector3F(1,1,1), Color.Yellow),
                new VertexColor(new Vector3F(1,1,-1), Color.Yellow),

                new VertexColor(new Vector3F(-1,-1,-1), Color.Purple), //bottom
                new VertexColor(new Vector3F(1,-1,1), Color.Purple),
                new VertexColor(new Vector3F(-1,-1,1), Color.Purple),
                new VertexColor(new Vector3F(-1,-1,-1), Color.Purple),
                new VertexColor(new Vector3F(1,-1,-1), Color.Purple),
                new VertexColor(new Vector3F(1,-1,1), Color.Purple),

                new VertexColor(new Vector3F(-1,-1,-1), Color.Green), //left
                new VertexColor(new Vector3F(-1,-1,1), Color.Green),
                new VertexColor(new Vector3F(-1,1,1), Color.Green),
                new VertexColor(new Vector3F(-1,-1,-1), Color.Green),
                new VertexColor(new Vector3F(-1,1,1), Color.Green),
                new VertexColor(new Vector3F(-1,1,-1), Color.Green),

                new VertexColor(new Vector3F(1,-1,-1), Color.White), //right
                new VertexColor(new Vector3F(1,1,1), Color.White),
                new VertexColor(new Vector3F(1,-1,1), Color.White),
                new VertexColor(new Vector3F(1,-1,-1), Color.White),
                new VertexColor(new Vector3F(1,1,-1), Color.White),
                new VertexColor(new Vector3F(1,1,1), Color.White),
            };

            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[0]);

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
