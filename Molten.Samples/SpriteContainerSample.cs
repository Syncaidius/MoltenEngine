using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SpriteContainerSample : SampleGame
    {
        public override string Description => "A sprite container demonstration.";
        IMesh<VertexTexture> _mesh;

        public SpriteContainerSample(EngineSettings settings = null) : base("Sprite Container", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);
        }

        protected override void OnContentRequested(ContentRequest cr)
        {
            cr.Load<IMaterial>("BasicTexture.sbm");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
        }

        protected override void OnContentLoaded(ContentRequest cr)
        {
            if (cr.RequestedFileCount == 0)
                return;

            IMaterial mat = cr.Get<IMaterial>("BasicTexture.sbm");
            if (mat == null)
            {
                Exit();
                return;
            }

            ITexture2D tex = cr.Get<ITexture2D>("dds_test.dds");
            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;
            SetupContainer();
        }

        private void SetupContainer()
        {
            SpriteBatchContainer container = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    sb.DrawRect(new Rectangle(500, 300, 250, 400), Color.Red, 0, new Vector2F(0.5f));
                    sb.DrawString(SampleFont, "Hello World!", new Vector2F(500, 100), Color.Yellow, new Vector2F(3));
                }
            };

            SpriteScene.AddObject(container);
        }

        protected override void OnUpdate(Timing time)
        {
            // TODO: Your update code here.

            base.OnUpdate(time);
        }
    }
}
