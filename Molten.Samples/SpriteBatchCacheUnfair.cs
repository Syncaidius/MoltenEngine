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
        IMesh<VertexTexture> _mesh;

        public SpriteBatchCacheUnfair(EngineSettings settings = null) : base("Sprite Batch Cache(Unfair)", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.sbm");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            if (cr.RequestedFileCount == 0)
                return;

            IMaterial mat = cr.Get<IMaterial>(0);
            if (mat == null)
            {
                Exit();
                return;
            }

            ITexture2D tex = cr.Get<ITexture2D>(1);
            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;

            UnfairSpriteCacheTestObject obj = new UnfairSpriteCacheTestObject(this, tex);
            SpriteScene.AddSprite(obj);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
