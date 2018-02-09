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
        List<Matrix> _positions;
        Camera2D _cam2D;
        IMesh<VertexTexture> _mesh;

        public SpriteBatchUnfairTest(EngineSettings settings = null) : base("Sprite Batch (Unfair)", settings)
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
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //front
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)), //back
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)), //top
               new VertexTexture(new Vector3(-1,1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)), //bottom
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //left
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)), //right
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
            };

            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3.Zero, out _parent, out _child);
        }

        /// <summary>Deliberately generate mixed up sprites to stress sprite-batch.</summary>
        /// <param name="tex"></param>
        private void SpamSprites(ITexture2D tex)
        {
            for (int i = 0; i < 50000; i++)
            {
                ISprite s;
                bool useTextured = Rng.Next(0, 5001) < 2500;
                if (useTextured)
                {
                    s = new Sprite()
                    {
                        Position = new Vector2()
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

                        Texture = tex,
                        Source = new Rectangle(0, 0, 128, 128),
                        Origin = new Vector2(0.5f),
                    };
                }
                else
                {
                    s = new RectangleSprite()
                    {
                        Destination = new Rectangle()
                        {
                            X = Rng.Next(0, 1920),
                            Y = Rng.Next(0, 1080),
                            Width = Rng.Next(16, 129),
                            Height = Rng.Next(16, 129)
                        },

                        Color = new Color()
                        {
                            R = (byte)Rng.Next(0, 255),
                            G = (byte)Rng.Next(0, 255),
                            B = (byte)Rng.Next(0, 255),
                            A = 40,
                        },

                        Origin = new Vector2(0.5f),
                    };
                }
                SampleScene.RenderData.AddSprite(s);
            }
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
            SpamSprites(tex);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
