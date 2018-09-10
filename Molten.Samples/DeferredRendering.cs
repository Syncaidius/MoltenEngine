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
        class ParentChildPair
        {
            public SceneObject Parent;
            public SceneObject Child;
        }

        public override string Description => "A test/sample for deferred rendering";

        List<ParentChildPair> _pairs;
        IMesh<GBufferVertex> _mesh;
        IMesh<GBufferVertex> _floorMesh;

        public DeferredRenderingSample(EngineSettings settings = null) : base("Deferred Rendering", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _pairs = new List<ParentChildPair>();
            _mesh = MeshHelper.Cube(engine.Renderer);

            Player.Transform.LocalPosition = new Vector3F(0, 3, -8);
            Player.Transform.LocalRotationX = 15;

            SpawnParentChildren(5, new Vector3F(0,2.5f,0), 10);

            SetupLightObjects(Vector3F.Zero);
            SetupFloor(Vector3F.Zero, 30);

            SceneCamera.Flags = RenderCameraFlags.Deferred;
            ContentRequest cr = engine.Content.BeginRequest("assets/");
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

        private void SpawnParentChildren(int count, Vector3F origin, float outerRadius)
        {
            ParentChildPair pair = new ParentChildPair();
            SpawnParentChild(_mesh, origin, out pair.Parent, out pair.Child);
            _pairs.Add(pair);

            float angle = 0;
            float angleIncrement = 360f / count;

            // Spawn more around the center pair
            for(int i = 0; i < count; i++)
            {
                pair = new ParentChildPair();
                float angRad = angle * MathHelper.DegToRad;
                Vector3F pos = origin + new Vector3F()
                {
                    X = (float)Math.Sin(angRad) * outerRadius,
                    Y = 0,
                    Z = (float)Math.Cos(angRad) * outerRadius,
                };
                SpawnParentChild(_mesh, pos, out pair.Parent, out pair.Child);
                pair.Parent.Transform.LocalRotationZ = Rng.Next(0, 360);
                pair.Parent.Transform.LocalRotationX = Rng.Next(0, 360);

                _pairs.Add(pair);
                angle += angleIncrement;
            }
        }

        private void SetupFloor(Vector3F origin, float size)
        {
            _floorMesh = MeshHelper.PlainCentered(Engine.Renderer, size / 4);
            SceneObject floorObj = CreateObject(origin, SampleScene);
            floorObj.Transform.LocalPosition = origin;
            floorObj.Transform.LocalScale = new Vector3F(size);

            MeshComponent floorCom = floorObj.AddComponent<MeshComponent>();
            floorCom.Mesh = _floorMesh;
        }

        private void SetupLightObjects(Vector3F origin)
        {
            int numLights = 5;
            float radius = 5.5f;

            float angInc = MathHelper.DegreesToRadians(360.0f / numLights);
            float angle = 0;

            for(int i = 0; i < numLights; i++)
            {
                Vector3F pos = origin + new Vector3F()
                {
                    X = (float)Math.Sin(angle) * radius,
                    Y = 2f,
                    Z = (float)Math.Cos(angle) * radius,
                };

                SceneObject obj = CreateObject(pos, SampleScene);
                PointLightComponent lightCom = obj.AddComponent<PointLightComponent>();
                lightCom.Range = radius + 1;
                lightCom.Intensity = 2.0f;
                lightCom.Color = new Color()
                {
                    R = (byte)Rng.Next(128, 255),
                    G = (byte)Rng.Next(128, 255),
                    B = (byte)Rng.Next(128, 255),
                };
                angle += angInc;
            }
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            if (cr.RequestedFileCount == 0)
                return;

            ITexture2D diffuseMap = cr.Get<ITexture2D>(0);
            _mesh.SetResource(diffuseMap, 0);

            ITexture2D normalMap = cr.Get<ITexture2D>(1);
            _mesh.SetResource(normalMap, 1);

            ITexture2D emssiveMap = cr.Get<ITexture2D>(2);
            _mesh.SetResource(emssiveMap, 2);
            
            diffuseMap = cr.Get<ITexture2D>(3);
            _floorMesh.SetResource(diffuseMap, 0);

            normalMap = cr.Get<ITexture2D>(4);
            _floorMesh.SetResource(normalMap, 1);

            emssiveMap = cr.Get<ITexture2D>(5);
            _floorMesh.SetResource(emssiveMap, 2);

            //ITexture2D specular = content.Get<ITexture2D>(cr[6]);
            //_floorMesh.SetResource(specular, 3);
        }

        protected override void OnUpdate(Timing time)
        {
            foreach (ParentChildPair pair in _pairs)
                RotateParentChild(pair.Parent, pair.Child, time);

            base.OnUpdate(time);
        }
    }
}
