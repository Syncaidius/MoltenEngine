using Molten.Collections;
using Molten.Graphics;
using static System.Formats.Asn1.AsnWriter;

namespace Molten
{
    /// <summary>Manages and controls a scene composed of 3D and 2D objects, while also feeding the needed data to a renderer if available. <para/>
    /// Multiple scenes can render to the same output camera if needed. This allows scenes to easily be onto the same surface. Ordering methods are provided for this purpose.</summary>
    public partial class Scene : EngineObject
    {
        internal SceneRenderData RenderData;
        internal List<SceneLayer> Layers;
    
        SceneLayer _defaultLayer;

        /// <summary>Creates a new instance of <see cref="Scene"/></summary>
        /// <param name="name">The name of the scene.</param>
        /// <param name="engine">The engine instance to which the scene will be bound.</param>
        public Scene(string name, Engine engine)
        {
            Name = name;
            Engine = engine;

            if(engine.Renderer != null)
                RenderData = engine.Renderer.CreateRenderData();

            engine.AddScene(this);

            Layers = new List<SceneLayer>();

            _defaultLayer = AddLayer("default");
            engine.Log.WriteLine($"Created scene '{name}'");
        }

        /// <summary>
        /// Adds a <see cref="SceneLayer"/> to the current <see cref="Scene"/>.
        /// </summary>
        /// <param name="name">The name of the layer.</param>
        /// <param name="ignoreRaycastHit">If true, any scene raycasts will ignore this layer.</param>
        /// <returns></returns>
        public SceneLayer AddLayer(string name, bool ignoreRaycastHit = false)
        {
            LayerRenderData layerData = null;

            if (RenderData != null)
            {
                layerData = RenderData.CreateLayerData(name);
                RenderData.AddLayer(layerData);
            }

            SceneLayerAdd change = SceneLayerAdd.Get();
            change.ParentScene = this;
            SceneLayer layer = new SceneLayer()
            {
                Name = name,
                IgnoreRaycastHit = ignoreRaycastHit,
                ParentScene = this,
                Data = layerData,
            };

            change.Layer = layer;
            Engine.Scenes.QueueChange(this, change);
            return layer;
        }

        /// <summary>Removes a <see cref="SceneLayer"/> from the current <see cref="Scene"/>.</summary>
        /// <param name="layer">The <see cref="SceneLayer"/> to be removed.</param>
        /// <exception cref="SceneLayerException"></exception>
        public void RemoveLayer(SceneLayer layer)
        {
            if (layer.ParentScene != this)
                throw new SceneLayerException(this, layer, "The provided layer does not belong to the current scene.");

            if (layer == _defaultLayer)
                throw new SceneLayerException(this, layer, "The default layer cannot be removed from a scene.");

            RenderData?.RemoveLayer(layer.Data);
            SceneLayerRemove change = SceneLayerRemove.Get();
            change.ParentScene = this;
            change.Layer = layer;
            Engine.Scenes.QueueChange(this, change);
        }

        internal void QueueLayerReorder(SceneLayer layer, ReorderMode mode)
        {
            SceneLayerReorder change = SceneLayerReorder.Get();
            change.Layer = layer;
            change.Mode = mode;
            Engine.Scenes.QueueChange(this, change);
        }

        /// <summary>
        /// Adds a new object to the current <see cref="Scene"/> with the specified <see cref="SceneComponent"/> attached to it.
        /// </summary>
        /// <typeparam name="C">The type of <see cref="SceneComponent"/>.</typeparam>
        /// <param name="layer">The layer on which to add the new object. If null, the default layer (0) will be used instead.</param>
        /// <param name="flags">The object update flags.</param>
        /// <param name="visible">Whether or not the object is spawned visible.</param>
        /// <returns>The <see cref="SceneComponent"/> which was added to the new object. It's parent object can be retrieved via <see cref="SceneComponent.Object"/>.</returns>
        public C AddObjectWithComponent<C>(SceneLayer layer = null, ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true) where C : SceneComponent, new()
        {
            SceneObject obj = new SceneObject(Engine, ObjectUpdateFlags.All, visible);
            C com = obj.Components.Add<C>();
            AddObject(obj, layer);

            return com;
        }

        /// <summary>
        /// Adds a new object to the current <see cref="Scene"/> with the specified <see cref="SceneComponent"/> attached to it.
        /// </summary>
        /// <param name="componentTypes">A list the type of each <see cref="SceneComponent"/> to be added to the object.</param>
        /// <param name="layer">The layer on which to add the new object. If null, the default layer (0) will be used instead.</param>
        /// <param name="flags">The object update flags.</param>
        /// <param name="visible">Whether or not the object is spawned visible.</param>
        /// <returns>The newly-created <see cref="SceneObject"/> containing all of the valid components that were specified.</returns>
        public SceneObject AddObjectWithComponents(IList<Type> componentTypes, SceneLayer layer = null, ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true)
        {
            SceneObject obj = new SceneObject(Engine, ObjectUpdateFlags.All, visible);
            Type comType = typeof(SceneComponent);

            for (int i = 0; i < componentTypes.Count; i++)
                obj.Components.Add(componentTypes[i]);

            AddObject(obj, layer);

            return obj;
        }

        /// <summary>Creates a new <see cref="SceneObject"/> at specified position.</summary>
        /// <param name="flags">The update flags to set on the newly spawned object.</param>
        /// <param name="visible">If true, the object is spawned as visible. False will spawn the object as hidden.</param>
        /// <returns></returns>
        public SceneObject CreateObject(ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true)
        {
            SceneObject obj = new SceneObject(Engine, flags, visible);
            obj.Transform.LocalPosition = Vector3F.Zero;
            AddObject(obj);
            return obj;
        }

        /// <summary>Creates a new <see cref="SceneObject"/> at specified position.</summary>
        /// <param name="position">The world position at which to create a new <see cref="SceneObject"/>.</param>
        /// <param name="flags">The update flags to set on the newly spawned object.</param>
        /// <param name="visible">If true, the object is spawned as visible. False will spawn the object as hidden.</param>
        /// <returns></returns>
        public SceneObject CreateObject(Vector3F position, ObjectUpdateFlags flags = ObjectUpdateFlags.All, bool visible = true)
        {
            SceneObject obj = new SceneObject(Engine, flags, visible);
            obj.Transform.LocalPosition = position;
            AddObject(obj);
            return obj;
        }

        /// <summary>Adds a <see cref="SceneObject"/> to the scene.</summary>
        /// <param name="obj">The object to be added.</param>
        public void AddObject(SceneObject obj, SceneLayer layer = null)
        {
            layer = layer ?? _defaultLayer;
            if (layer.ParentScene != this)
                throw new SceneLayerException(this, layer, "The provided layer does not belong to the current scene.");

            SceneAddObject change = SceneAddObject.Get();
            change.Object = obj;
            change.Layer = layer;
            Engine.Scenes.QueueChange(this, change);
        }

        /// <summary>Removes a <see cref="SceneObject"/> from the scene.</summary>
        /// <param name="obj">The object to be removed.</param>
        /// <param name="layer">The layer from which to remove the object. Must belong to the current <see cref="Scene"/> instance.</param>
        public void RemoveObject(SceneObject obj, SceneLayer layer = null)
        {
            layer = layer ?? _defaultLayer;

            if (layer.ParentScene != this)
                throw new SceneLayerException(this, layer, "The provided layer does not belong to the current scene.");

            SceneRemoveObject change = SceneRemoveObject.Get();
            change.Object = obj;
            change.Layer = layer;
            Engine.Scenes.QueueChange(this, change);
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">A <see cref="Timing"/> instance.</param>
        internal void Update(Timing time)
        {
            foreach (SceneLayer layer in Layers)
            {
                foreach (SceneObject up in layer.Objects)
                {
                    if (up.IsEnabled)
                        up.Update(time);
                }
            }
        }

        protected override void OnDispose()
        {
            Engine.RemoveScene(this);
            Engine.Renderer?.DestroyRenderData(RenderData);
            Engine.Log.WriteLine($"Destroyed scene '{Name}'");
        }

        /// <summary>Gets or sets whether or not the scene is updated.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Gets or sets whether the scene is rendered.</summary>
        public bool IsVisible
        {
            get => RenderData?.IsVisible ?? false;
            set
            {
                if (RenderData != null)
                    RenderData.IsVisible = value;
            }
        }

        /// <summary>Gets the <see cref="Engine"/> instance that the <see cref="Scene"/> is bound to.</summary>
        public Engine Engine { get; private set; }

        /// <summary>Gets or sets the background color of the scene.</summary>
        public Color BackgroundColor
        {
            get => RenderData?.BackgroundColor ?? Color.Transparent;
            set
            {
                if(RenderData != null)
                    RenderData.BackgroundColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the ambient light color of the scene.
        /// </summary>
        public Color AmbientColor
        {
            get => RenderData?.AmbientLightColor ?? Color.Transparent;
            set
            {
                if(RenderData != null)
                    RenderData.AmbientLightColor = value;
            }
        }

        /// <summary>
        /// Gets the sccene's default layer. This cannot be removed from the scene.
        /// </summary>
        public SceneLayer DefaultLayer => _defaultLayer;

        /// <summary>
        /// Gets the scene's debug overlay. 
        /// </summary>
        public RenderProfiler Profiler => RenderData?.Profiler;

        /// <summary>
        /// Gets or sets the scene's skybox texture.
        /// </summary>
        public ITextureCube SkyboxTeture
        {
            get => RenderData?.SkyboxTexture;
            set
            {
                if(RenderData != null)
                {
                    RenderData.SkyboxTexture = value;
                }
            }
        }
    }
}
