using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SpriteBatchUnfairTest : SampleSceneGame
    {
        public override string Description => "A stress test of sprite batching which deliberately draws unsorted/jumbled sprites in random order to cause crazy amounts of draw calls.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexTexture> _mesh;
        List<Sprite> _sprites;

        public SpriteBatchUnfairTest(EngineSettings settings = null) : base("Sprite Batch (Unfair)", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        /// <summary>Deliberately generate mixed up sprites to stress sprite-batch.</summary>
        /// <param name="tex"></param>
        private void SetupSprites(ITexture2D tex)
        {
            _sprites = new List<Sprite>();

            for (int i = 0; i < 50000; i++)
            {
                _sprites.Add(new Sprite()
                {
                    Position = new Vector2F()
                    {
                        X = Rng.Next(0, 1920),
                        Y = Rng.Next(0, 1080),
                    },

                    Color = new Color()
                    {
                        R = (byte)Rng.Next(0, 255),
                        G = (byte)Rng.Next(0, 255),
                        B = (byte)Rng.Next(0, 255),
                        A = (byte)Rng.Next(0, 255),
                    },

                    Texture = Rng.Next(0, 5001) < 2500 ? tex : null,
                    Source = new Rectangle(0, 0, 128, 128),
                    Origin = new Vector2F(0.5f),
                });
            }

            SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = (sb) =>
            {
                for (int i = 0; i < _sprites.Count; i++)
                    sb.Draw(_sprites[i]);
            };
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
            if (tex != null)
            {
                mat.SetDefaultResource(tex, 0);
                _mesh.Material = mat;
                SetupSprites(tex);
            }            
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
