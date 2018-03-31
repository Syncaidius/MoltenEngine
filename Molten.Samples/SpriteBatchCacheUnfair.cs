using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SpriteBatchCacheUnfair : SampleSceneGame
    {
        public override string Description => "An unfair stress test for sprite batch caching. Deliberately ignores sprite texture ordering in order to drag down performance.";

        SceneObject _parent;
        SceneObject _child;
        List<Matrix4F> _positions;
        Camera2D _cam2D;
        ICustomMesh<VertexTexture> _mesh;

        public SpriteBatchCacheUnfair(EngineSettings settings = null) : base("Sprite Batch Cache(Unfair)", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _cam2D = new Camera2D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicTexture.sbm");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateCustomMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //front
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)), //back
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),

               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)), //top
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)), //bottom
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),

               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //left
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)), //right
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
            };

            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }



        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[0]);
            if (mat == null)
            {
                Exit();
                return;
            }

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[1]);
            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;

            UnfairSpriteCacheTestObject obj = new UnfairSpriteCacheTestObject(this, tex);
            SampleScene.AddSprite(obj);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
