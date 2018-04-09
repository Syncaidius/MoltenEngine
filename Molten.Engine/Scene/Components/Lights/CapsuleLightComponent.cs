using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class CapsuleLightComponent : SceneComponent
    {
        CapsuleLightData _data;
        int _lightID = -1;
        bool _visible = false;
        float _range;

        protected override void OnInitialize(SceneObject obj)
        {
            _range = 1.0f;

            _data = new CapsuleLightData()
            {
                Color = Color.White,
                Intensity = 1.0f,
                Position = Vector3F.Zero,
                RangeRcp = 1.0f / _range,
                TessFactor = GraphicsSettings.MIN_LIGHT_TESS_FACTOR,
            };

            AddToScene(obj);
            obj.OnRemovedFromScene += Obj_OnRemovedFromScene;
            obj.OnAddedToScene += Obj_OnAddedToScene;

            base.OnInitialize(obj);
        }

        private void AddToScene(SceneObject obj)
        {
            if (_lightID > -1)
                return;

            // Add mesh to render data if possible.
            if (_visible && obj.Scene != null)
                _lightID = obj.Scene.RenderData.CapsuleLights.Add(_data);
        }

        private void RemoveFromScene(SceneObject obj)
        {
            if (_lightID == -1)
                return;

            if (obj.Scene != null || _visible)
            {
                obj.Scene.RenderData.PointLights.Remove(_lightID);
                _lightID = -1;
            }
        }

        protected override void OnDestroy(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            // Reset State
            _lightID = -1;
            _visible = true;

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
            if (_lightID > -1)
            {
                ICamera cam = Object.Scene.OutputCamera;
                _data.Position = Object.Transform.GlobalPosition;
                float distFromCam = Vector3F.Distance(cam.View.Translation, _data.Position);
                float distPercent = Math.Min(1.0f, distFromCam / cam.MaximumDrawDistance);
                _data.TessFactor = Math.Max(GraphicsSettings.MIN_LIGHT_TESS_FACTOR, GraphicsSettings.MAX_LIGHT_TESS_FACTOR - (GraphicsSettings.MAX_LIGHT_TESS_FACTOR * distPercent));
                _data.Transform = Matrix4F.Scaling(_range) * Matrix4F.CreateTranslation(_data.Position) * cam.ViewProjection;
                _data.Transform.Transpose();
                Object.Scene.RenderData.CapsuleLights.Data[_lightID] = _data;
            }
        }

        /// <summary>
        /// Gets or sets whether the light is visible.
        /// </summary>
        public override bool IsVisible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;

                    if (_visible)
                        AddToScene(Object);
                    else
                        RemoveFromScene(Object);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        public Color Color
        {
            get => _data.Color;
            set => _data.Color = value;
        }

        /// <summary>
        /// Gets or sets the light's range or radius. Default is 1.0f.
        /// </summary>
        public float Range
        {
            get => _range;
            set
            {
                _range = value;
                _data.RangeRcp = 1.0f / _range;
            }
        }

        /// <summary>
        /// Gets or sets the light intensity. Default is 1.0f.
        /// </summary>
        public float Intensity
        {
            get => _data.Intensity;
            set => _data.Intensity = value;
        }

        /// <summary>
        /// Gets or sets the length of the capsule light.
        /// </summary>
        public float Length
        {
            get => _data.Length;
            set => _data.Length = value;
        }
    }
}
