using Molten.Graphics;
using Molten.Input;

namespace Molten
{
    /// <summary>An implementation of <see cref="IInputCamera"/> which provides a left-handed projection matrix based on it's <see cref="IInputCamera.Surface"/>.</summary>
    public class CameraComponent : SceneComponent, IInputCamera
    {
        RenderCamera _camera;
        bool _inScene = false;

        /// <summary>
        /// Occurs when the output surface has been changed.
        /// </summary>
        public event InputCameraSurfaceHandler OnSurfaceChanged;

        public event InputCameraSurfaceHandler OnSurfaceResized;

        /// <summary>
        /// Creates a new instance of <see cref="CameraComponent"/>
        /// </summary>
        public CameraComponent()
        {
            _camera = new RenderCamera(RenderCameraMode.Perspective);
            _camera.OnOutputSurfaceChanged += _camera_OnOutputSurfaceChanged;
            _camera.OnSurfaceResized += _camera_OnSurfaceResized;
        }

        private void _camera_OnSurfaceResized(RenderCamera camera, IRenderSurface2D surface)
        {
            OnSurfaceResized?.Invoke(this, surface);
        }

        protected override void OnDispose() { }

        private void _camera_OnOutputSurfaceChanged(RenderCamera camera, IRenderSurface2D oldSurface, IRenderSurface2D newSurface)
        {
            OnSurfaceChanged?.Invoke(this, newSurface);
        }

        protected override void OnInitialize(SceneObject obj)
        {
            AddToScene(obj);
            obj.OnRemovedFromScene += Obj_OnRemovedFromScene;
            obj.OnAddedToScene += Obj_OnAddedToScene;

            base.OnInitialize(obj);
        }

        private void AddToScene(SceneObject obj)
        {
            if (_inScene)
                return;

            // Add mesh to render data if possible.
            if (obj.Scene != null)
            {
                obj.Scene.RenderData.AddObject(_camera);
                _inScene = true;
            }
        }

        private void RemoveFromScene(SceneObject obj)
        {
            if (!_inScene)
                return;

            if (obj.Scene != null)
            {
                obj.Scene.RenderData.RemoveObject(_camera);
                _inScene = false;
            }
        }

        protected internal override bool OnRemove(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            return base.OnRemove(obj);
        }

        private void Obj_OnAddedToScene(SceneObject obj, Scene scene, SceneLayer layer)
        {
            AddToScene(obj);
        }

        private void Obj_OnRemovedFromScene(SceneObject obj, Scene scene, SceneLayer layer)
        {
            RemoveFromScene(obj);
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            _camera.Transform = Object.Transform.Global;
        }

        /// <summary>Converts the provided screen position to a globalized 3D world position.</summary>
        /// <param name="location">The screen position.</param>
        /// <returns></returns>
        public Vector3F ConvertScreenToWorld(Vector2F location)
        {
            Vector4F result = Vector2F.Transform(location, Object.Transform.Global);
            return new Vector3F(result.X, result.Y, result.Z);
        }

        public Vector2F ConvertWorldToScreen(Vector3F position)
        {
            Vector4F result = Vector3F.Transform(position, _camera.View);
            return new Vector2F(result.X, result.Y);
        }

        /// <summary>
        /// Returns whether or not the current <see cref="CameraComponent"/> has the specified <see cref="RenderCameraFlags"/>.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        public bool HasFlags(RenderCameraFlags flags)
        {
            return _camera.HasFlags(flags);
        }

        /// <summary>
        /// Gets the view matrix of the current <see cref="CameraComponent"/>.
        /// </summary>
        public Matrix4F View => _camera.View;

        /// <summary>
        /// Gets the projection matrix of the current <see cref="CameraComponent"/>.
        /// </summary>
        public Matrix4F Projection => _camera.Projection;

        /// <summary>
        /// Gets the combined view and projection matrix of the current <see cref="CameraComponent"/>. This is the result of multiplying <see cref="View"/> and <see cref="Projection"/> together.
        /// </summary>
        public Matrix4F ViewProjection => _camera.ViewProjection;

        /// <summary>Gets or sets the <see cref="IRenderSurface2D"/> that the camera's view should be rendered out to.</summary>
        public IRenderSurface2D Surface
        {
            get => _camera.Surface;
            set => _camera.Surface = value;
        }

        /// <summary>Gets or sets the minimum draw dinstance. Also known as the near-clip plane. 
        /// Anything closer this value will not be drawn.</summary>
        public float MinDrawDistance
        {
            get => _camera.MinDrawDistance;
            set => _camera.MinDrawDistance = value;
        }

        /// <summary>Gets or sets the maximum draw distance. Also known as the far-clip plane. 
        /// Anything further away than this value will not be drawn.</summary>
        public float MaxDrawDistance
        {
            get => _camera.MaxDrawDistance;
            set => _camera.MaxDrawDistance = value;
        }

        /// <summary>
        /// Gets or sets the camera's field-of-view (FoV), in radians.
        /// </summary>
        public float FieldOfView
        {
            get => _camera.FieldOfView;
            set => _camera.FieldOfView = value;
        }

        /// <summary>
        /// Gets the position of the camera based on it's parent object's transform.
        /// </summary>
        public Vector3F Position => Object.Transform.GlobalPosition;

        public RenderCameraFlags Flags
        {
            get => _camera.Flags;
            set => _camera.Flags = value;
        }

        /// <summary>
        /// Gets or sets the camera's layer render mask. Each enabled bit ignores a layer with the same ID as the bit's position. 
        /// For example, setting bit 0 will skip rendering of layer 0 (the default layer).
        /// </summary>
        public SceneLayerMask LayerMask
        {
            get => _camera.LayerMask;
            set => _camera.LayerMask = value;
        }

        /// <summary>
        /// Gets or sets the ordering depth of the current <see cref="RenderCamera"/>. The default value is 0.
        /// Cameras which share the same output surface and order-depth will be rendered in the other they were added to the scene.
        /// If you intend to output multiple cameras to the same <see cref="IRenderSurface2D"/>, it is recommended you change the order depth accordingly.
        /// </summary>
        public int OrderDepth
        {
            get => _camera.OrderDepth;
            set => _camera.OrderDepth = value;
        }

        /// <summary>
        /// Gets or sets the camera's mode.
        /// </summary>
        public RenderCameraMode Mode
        {
            get => _camera.Mode;
            set => _camera.Mode = value;
        }

        public AntiAliasLevel MultiSampleLevel
        {
            get => _camera.MultiSampleLevel;
            set => _camera.MultiSampleLevel = value;
        }
    }
}
