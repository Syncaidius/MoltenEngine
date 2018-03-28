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

            _mesh = MeshHelper.Cube(engine.Renderer);

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
