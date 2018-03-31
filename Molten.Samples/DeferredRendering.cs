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
        ICustomMesh<GBufferVertex> _mesh;

        public DeferredRenderingSample(EngineSettings settings = null) : base("Deferred Rendering", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            SampleScene.RenderData.Flags |= SceneRenderFlags.Deferred;
            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.Load<IMaterial>("gbuffer.sbm"); // TODO TEMP until the shader is used by default from within the renderer.
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = MeshHelper.Cube(engine.Renderer);

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[0]);
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[1]);

            if (mat == null)
            {
                Exit();
                return;
            }

            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
