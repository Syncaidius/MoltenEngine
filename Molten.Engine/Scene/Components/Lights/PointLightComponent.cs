using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class PointLightComponent : SceneComponent
    {
        LightData _data;
        LightInstance _instance;
        bool _visible = true;
        float _range;
        Color _color;

        protected override void OnInitialize(SceneObject obj)
        {
            _range = 1.0f;
            _color = Color.White;

            _data = new LightData()
            {
                Color3 = _color.ToColor3(),
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
            if (_instance != null)
                return;

            // Add mesh to render data if possible.
            if (_visible && obj.Scene != null)
                _instance = obj.Scene.RenderData.PointLights.New(_data);
        }

        private void RemoveFromScene(SceneObject obj)
        {
            if (_instance == null)
                return;

            if (obj.Scene != null || _visible)
            {
                obj.Scene.RenderData.PointLights.Remove(_instance);
                _instance = null;
            }
        }

        protected override void OnDestroy(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            // Reset State
            _instance = null;
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
            if (_instance != null)
            {
                _data.Position = Object.Transform.GlobalPosition;
                Object.Scene.RenderData.PointLights.Data[_instance.ID] = _data;
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
            get => _color;
            set
            {
                _color = value;
                _data.Color3 = _color.ToColor3();
            }
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
                _data.RangeRcp = 1.0f / (_range * _range);
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
    }
}
