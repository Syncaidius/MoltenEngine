using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Deferred Rendering", "[WIP] Showcases Molten's deferred rendering capabilities")]
    public class DeferredRendering : MoltenExample
    {
        class ParentChildPair
        {
            public SceneObject Parent;
            public SceneObject Child;
        }

        ContentLoadHandle _hTex;
        ContentLoadHandle _hTexNormal;
        ContentLoadHandle _hTexEmissive;
        ContentLoadHandle _hTexMetal;
        ContentLoadHandle _hTexMetalNormal;
        ContentLoadHandle _hTexMetalEmissive;
        ContentLoadHandle _hTexSkybox;

        List<ParentChildPair> _pairs;
        IMesh<GBufferVertex> _mesh;
        IMesh<GBufferVertex> _floorMesh;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hTex = loader.Load<ITexture2D>("assets/dds_test.dds");
            _hTexNormal = loader.Load<ITexture2D>("assets/dds_test_n.dds");
            _hTexEmissive = loader.Load<ITexture2D>("assets/dds_test_e.dds");
            _hTexMetal = loader.Load<ITexture2D>("assets/metal.dds");
            _hTexMetalNormal = loader.Load<ITexture2D>("assets/metal_n.dds");
            _hTexMetalEmissive = loader.Load<ITexture2D>("assets/metal_e.dds");
            _hTexSkybox = loader.Load<ITextureCube>("assets/cubemap.dds");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            ITexture2D diffuseMap = _hTex.Get<ITexture2D>();
            _mesh.SetResource(diffuseMap, 0);

            ITexture2D normalMap = _hTexNormal.Get<ITexture2D>();
            _mesh.SetResource(normalMap, 1);

            ITexture2D emssiveMap = _hTexEmissive.Get<ITexture2D>();
            _mesh.SetResource(emssiveMap, 2);

            SetupFloor(Vector3F.Zero, 30);

            diffuseMap = _hTexMetal.Get<ITexture2D>();
            _floorMesh.SetResource(diffuseMap, 0);

            normalMap = _hTexMetalNormal.Get<ITexture2D>();
            _floorMesh.SetResource(normalMap, 1);

            emssiveMap = _hTexMetalEmissive.Get<ITexture2D>();
            _floorMesh.SetResource(emssiveMap, 2);

            MainScene.SkyboxTeture = _hTexSkybox.Get<ITextureCube>();

            Player.Transform.LocalPosition = new Vector3F(0, 3, -8);
            Player.Transform.LocalRotationX = 15;

            SpawnParentChildren(5, new Vector3F(0, 2.5f, 0), 10);
            SetupLightObjects(Vector3F.Zero);

            SceneCamera.Flags = RenderCameraFlags.Deferred;
        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _pairs = new List<ParentChildPair>();
            _mesh = MeshHelper.Cube(engine.Renderer);
        }

        private void SpawnParentChildren(int count, Vector3F origin, float outerRadius)
        {
            ParentChildPair pair = new ParentChildPair();
            SpawnParentChild(_mesh, origin, out pair.Parent, out pair.Child);
            _pairs.Add(pair);

            float angle = 0;
            float angleIncrement = 360f / count;

            // Spawn more around the center pair
            for (int i = 0; i < count; i++)
            {
                pair = new ParentChildPair();
                float angRad = angle * MathHelper.Constants<float>.DegToRad;
                Vector3F pos = origin + new Vector3F()
                {
                    X = MathF.Sin(angRad) * outerRadius,
                    Y = 0,
                    Z = MathF.Cos(angRad) * outerRadius,
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
            SceneObject floorObj = MainScene.CreateObject(origin);
            floorObj.Transform.LocalPosition = origin;
            floorObj.Transform.LocalScale = new Vector3F(size);

            MeshComponent floorCom = floorObj.Components.Add<MeshComponent>();
            floorCom.RenderedObject = _floorMesh;
        }

        private void SetupLightObjects(Vector3F origin)
        {
            int numLights = 5;
            float radius = 5.5f;

            float angInc = MathHelper.DegreesToRadians(360.0f / numLights);
            float angle = 0;

            for (int i = 0; i < numLights; i++)
            {
                Vector3F pos = origin + new Vector3F()
                {
                    X = MathF.Sin(angle) * radius,
                    Y = 2f,
                    Z = MathF.Cos(angle) * radius,
                };

                PointLightComponent lightCom = MainScene.AddObjectWithComponent<PointLightComponent>();
                lightCom.Object.Transform.LocalPosition = pos;
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

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            foreach (ParentChildPair pair in _pairs)
                RotateParentChild(pair.Parent, pair.Child, time);
        }
    }
}
