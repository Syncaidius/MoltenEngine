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
        IMesh<GBufferVertex> _floorMesh;

        public DeferredRenderingSample(EngineSettings settings = null) : base("Deferred Rendering", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _mesh = MeshHelper.Cube(engine.Renderer);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
            AcceptPlayerInput = false;
            Player.Transform.LocalPosition = new Vector3F(0, 3, -8);
            Player.Transform.LocalRotationX = -15;

            SetupLightObjects(Vector3F.Zero);
            SetupFloor(Vector3F.Zero);

            SampleScene.RenderFlags = SceneRenderFlags.Deferred | SceneRenderFlags.Render3D;
            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("dds_test.dds");
            cr.Load<ITexture2D>("dds_test_n.dds");
            cr.Load<ITexture2D>("dds_test_e.dds");
            cr.Load<ITexture2D>("metal.dds");
            cr.Load<ITexture2D>("metal_n.dds");
            cr.Load<ITexture2D>("metal_e.dds");
            cr.Load<ITexture2D>("metal_s.dds");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        private void SetupFloor(Vector3F origin)
        {
            _floorMesh = MeshHelper.PlainCentered(Engine.Renderer, 3);
            SceneObject floorObj = CreateObject(origin, SampleScene);
            floorObj.Transform.LocalPosition = origin;
            floorObj.Transform.LocalScale = new Vector3F(10);

            MeshComponent floorCom = floorObj.AddComponent<MeshComponent>();
            floorCom.Mesh = _floorMesh;
        }

        private void SetupLightObjects(Vector3F origin)
        {
            int numLights = 10;
            float radius = 5.5f;

            float angInc = MathHelper.DegreesToRadians(360.0f / numLights);
            float angle = 0;

            for(int i = 0; i < numLights; i++)
            {
                Vector3F pos = origin + new Vector3F()
                {
                    X = (float)Math.Sin(angle) * radius,
                    Y = 1f,
                    Z = (float)Math.Cos(angle) * radius,
                };

                SceneObject obj = CreateObject(pos, SampleScene);
                PointLightComponent lightCom = obj.AddComponent<PointLightComponent>();
                lightCom.Range = radius + 1;
                lightCom.Intensity = 2.0f;

                angle += angInc;
            }
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
            
            diffuseMap = content.Get<ITexture2D>(cr.RequestedFiles[3]);
            _floorMesh.SetResource(diffuseMap, 0);

            normalMap = content.Get<ITexture2D>(cr.RequestedFiles[4]);
            _floorMesh.SetResource(normalMap, 1);

            emssiveMap = content.Get<ITexture2D>(cr.RequestedFiles[5]);
            _floorMesh.SetResource(emssiveMap, 2);

            //ITexture2D specular = content.Get<ITexture2D>(cr.RequestedFiles[6]);
            //_floorMesh.SetResource(specular, 3);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
