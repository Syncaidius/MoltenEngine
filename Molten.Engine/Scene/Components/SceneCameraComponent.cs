using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>An implementation of <see cref="Camera"/> which provides a left-handed projection matrix based on it's <see cref="Camera.OutputSurface"/>.</summary>
    public class SceneCameraComponent : SceneComponent
    {
        RenderCamera _camera;
        bool _inScene = false;

        public SceneCameraComponent()
        {
            _camera = new RenderCamera(RenderCameraPreset.Perspective);
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

        protected override void OnDestroy(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            base.OnDestroy(obj);
        }

        private void Obj_OnAddedToScene(SceneObject obj, Scene scene)
        {
            AddToScene(obj);
        }

        private void Obj_OnRemovedFromScene(SceneObject obj, Scene scene)
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

        public void SetProjectionPreset(RenderCameraPreset preset)
        {
            _camera.SetProjectionPreset(preset);
        }

        public void SetProjectionFunc(RenderCameraProjectionFunc func)
        {
            _camera.SetProjectionFunc(func);
        }

        public Matrix4F View => _camera.View;

        public Matrix4F Projection => _camera.Projection;

        public Matrix4F ViewProjection => _camera.ViewProjection;

        /// <summary>Gets or sets the <see cref="IRenderSurface"/> that the camera's view should be rendered out to.</summary>
        public IRenderSurface OutputSurface
        {
            get => _camera.OutputSurface;
            set => _camera.OutputSurface = value;
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
    }
}
