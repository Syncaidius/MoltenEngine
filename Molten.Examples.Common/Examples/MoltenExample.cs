using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Molten.Input;
using Molten.UI;

namespace Molten.Examples
{
    public abstract class MoltenExample
    {

        public event ObjectHandler<MoltenExample> Closed;

        ContentLoadBatch _loader;

        SceneObject _parent;
        SceneObject _child;
        Foundation _foundation;
        UITexture _uiSurface;

        public void Initialize(Foundation foundation, SpriteFont font, IRenderSurface2D surface, Logger log)
        {
            _foundation = foundation;
            Engine = foundation.Engine;
            Surface = surface;
            Font = font;
            Log = log;

            MainScene = new Scene($"Example_{GetType().Name}", Engine);
            MainScene.BackgroundColor = new Color()
            {
                R = (byte)Rng.Next(0,256),
                G = (byte)Rng.Next(0, 256),
                B = (byte)Rng.Next(0, 256),
                A = 255,
            };//new Color(0x333333);
            SpriteLayer = MainScene.AddLayer("sprite", true);
            UILayer = MainScene.AddLayer("ui", true);
            UILayer.BringToFront();
            UI = UILayer.AddObjectWithComponent<UIManagerComponent>();

            SampleSpriteRenderComponent spriteCom = UILayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            spriteCom.RenderCallback = DrawSprites;
            spriteCom.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;

            // Use the same camera for both the sprite and UI scenes.
            Camera2D = MainScene.AddObjectWithComponent<CameraComponent>(UILayer);
            Camera2D.Mode = RenderCameraMode.Orthographic;
            Camera2D.OrderDepth = 1;
            Camera2D.MaxDrawDistance = 1.0f;
            Camera2D.Surface = Surface;
            Camera2D.LayerMask = SceneLayerMask.Layer0;
            Camera2D.OnSurfaceChanged += UpdateUIRootBounds;
            Camera2D.OnSurfaceResized += UpdateUIRootBounds;

            UI.Root.IsScrollingEnabled = false;

            TestMesh = GetTestCubeMesh();
            SpawnPlayer();

            OnInitialize(Engine);

            _loader = Engine.Content.GetLoadBatch();
            OnLoadContent(_loader);
            _loader.OnCompleted += _loader_OnCompleted;
            _loader.Dispatch();
        }
        private void UpdateUIRootBounds(CameraComponent camera, IRenderSurface2D surface)
        {
            UI.Root.LocalBounds = new Rectangle(0, 0, (int)surface.Width, (int)surface.Height);
        }

        public void Close()
        {
            // TODO unload assets stored in _loader
            Closed?.Invoke(this);
            IsLoaded = false;
        }

        private void _loader_OnCompleted(ContentLoadBatch loader)
        {
            // TODO hide loading screen

            SpawnParentChild(TestMesh, Vector3F.Zero, out _parent, out _child);
            IsLoaded = true;
            UpdateUIRootBounds(Camera2D, Camera2D.Surface);
        }

        private void SpawnPlayer()
        {
            Player = MainScene.CreateObject();
            Player.Transform.LocalPosition = new Vector3F(0, 0, -10);
            SceneCamera = Player.Components.Add<CameraComponent>();
            CameraController = Player.Components.Add<SampleCameraController>();
            SceneCamera.LayerMask = SceneLayerMask.Layer1 | SceneLayerMask.Layer2;
            SceneCamera.Surface = Surface;
            SceneCamera.MaxDrawDistance = 300;
            //SceneCamera.MultiSampleLevel = AntiAliasLevel.X8;
            MainScene.AddObject(Player);
        }

        protected virtual IMesh GetTestCubeMesh()
        {
            IMesh<VertexTexture> cube = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            cube.SetVertices(SampleVertexData.TexturedCube);
            return cube;
        }

        private SceneObject SpawnTestCube(IMesh mesh, Vector3F pos)
        {
            SceneObject obj = MainScene.CreateObject(pos);
            MeshComponent meshCom = obj.Components.Add<MeshComponent>();
            meshCom.RenderedObject = mesh;
            return obj;
        }

        protected void SpawnParentChild(IMesh mesh, Vector3F origin, out SceneObject parent, out SceneObject child)
        {
            parent = SpawnTestCube(mesh, Vector3F.Zero);
            child = SpawnTestCube(mesh, Vector3F.Zero);

            child.Transform.LocalScale = new Vector3F(0.5f);
            child.Transform.LocalPosition = new Vector3F(0, 0, 2);
            parent.Transform.LocalPosition = origin;
            parent.Children.Add(child);
        }

        protected void RotateParentChild(SceneObject parent, SceneObject child, Timing time, float speed = 0.5f, float childSpeed = 1.0f)
        {
            var rotateTime = (float)time.TotalTime.TotalSeconds;

            parent.Transform.LocalRotationY += speed;
            if (parent.Transform.LocalRotationY >= 360)
                parent.Transform.LocalRotationY -= 360;

            child.Transform.LocalRotationX += childSpeed;
            if (child.Transform.LocalRotationX >= 360)
                child.Transform.LocalRotationX -= 360;
        }

        public void Update(Timing time)
        {
            CameraController.AcceptInput = IsFocused;

            // Don't update until the base content is loaded.
            if (_loader.Status != ContentLoadBatchStatus.Completed)
            {
                // TODO update loading screen
                return;
            }

            if (_child != null)
                RotateParentChild(_parent, _child, time);

            OnUpdate(time);
        }

        protected void DrawSprites(SpriteBatcher sb)
        {
            if (!IsLoaded)
                return;

            OnDrawSprites(sb);
        }

        protected virtual void OnDrawSprites(SpriteBatcher sb) { }

        protected virtual void OnInitialize(Engine engine) { }

        protected virtual void OnLoadContent(ContentLoadBatch loader) { }

        protected virtual void OnUpdate(Timing time) { }

        public SpriteFont Font { get; private set; }

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();

        /// <summary>
        /// Gets the sample's UI scene layer.
        /// </summary>
        public SceneLayer UILayer { get; private set; }

        /// <summary>
        /// Gets the sample's sprite scene layer.
        /// </summary>
        public SceneLayer SpriteLayer { get; private set; }

        public UIManagerComponent UI { get; private set; }

        /// <summary>
        /// Gets the sample's sprite scene. This is rendered before <see cref="UIScene"/>.
        /// </summary>
        public Scene MainScene { get; private set; }

        protected IMesh TestMesh { get; private set; }

        public SceneObject Player { get; private set; }

        public CameraComponent SceneCamera { get; set; }

        public CameraComponent Camera2D { get; set; }

        public SampleCameraController CameraController { get; private set; }

        public Engine Engine { get; private set; }

        public IRenderSurface2D Surface { get; private set; }

        protected Logger Log { get; private set; }

        public bool IsLoaded { get; private set; }

        public bool IsFocused { get; set; }

        protected KeyboardDevice Keyboard => _foundation.Keyboard;

        protected MouseDevice Mouse => _foundation.Mouse;

        protected GamepadDevice Gamepad => _foundation.Gamepad;
    }
}
