using Molten.Collections;
using Molten.Graphics;
using System.Reflection;

namespace Molten
{
    public delegate void SceneObjectHandler(SceneObject obj);
    public delegate void SceneObjectVisibilityHandler(SceneObject obj, bool visible);
    public delegate void SceneObjectSceneHandler(SceneObject obj, Scene scene, SceneLayer layer);
    public delegate void SceneObjectLayerHandler(SceneObject obj, SceneLayer oldLayer, SceneLayer newLayer);

    public sealed class SceneObject : EngineObject
    {
        Engine _engine;
        Scene _scene;
        SceneLayer _layer;

        // Transform-related variables
        SceneObjectTransform _transform;
        ObjectUpdateFlags _updateFlags;
        bool _visible;

        Dictionary<Type, List<SceneComponent>> _componentsByType;
        ThreadedList<SceneComponent> _components;
        SceneChildCollection _children;

        public event SceneObjectVisibilityHandler OnVisibilityChanged;
        public event SceneObjectHandler OnUpdateFlagsChanged;

        /// <summary>
        /// Invoked when the object is added to a scene.
        /// </summary>
        public event SceneObjectSceneHandler OnAddedToScene;

        /// <summary>
        /// Occus when the object is removed from a scene.
        /// </summary>
        public event SceneObjectSceneHandler OnRemovedFromScene;

        /// <summary>Creates a new instance of <see cref="SceneObject"/>.</summary>
        /// <param name="engine">The <see cref="Molten.Engine"/> instance that the object will be bound to.</param>
        /// <param name="updateFlags">The object's initial update flags.</param>
        /// <param name="visible">Decides whether the object is initially visible when spawned. The default value is true.</param>
        internal SceneObject(Engine engine, ObjectUpdateFlags updateFlags = ObjectUpdateFlags.Children | ObjectUpdateFlags.Self, bool visible = true)
        {
            _engine = engine;
            _components = new ThreadedList<SceneComponent>();
            _componentsByType = new Dictionary<Type, List<SceneComponent>>();
            _children = new SceneChildCollection(this);
            _transform = new SceneObjectTransform(this);

            _children.OnAdded += _children_OnItemAdded;
            _children.OnRemoved += _children_OnItemRemoved;

            _updateFlags = updateFlags;
            IsVisible = visible;
        }

        protected override void OnDispose() { }

        private void _children_OnItemRemoved(SceneChildCollection collection, SceneObject item)
        {
            item.Parent = null;
        }

        private void _children_OnItemAdded(SceneChildCollection collection, SceneObject item)
        {
            item.Parent = this;
        }

        public T AddComponent<T>() where T : SceneComponent, new()
        {
            Type t = typeof(T);
            T component = Activator.CreateInstance(t) as T;
            _components.Add(component);
            component.Initialize(this);

            List<SceneComponent> comByType;
            if (!_componentsByType.TryGetValue(t, out comByType))
            {
                comByType = new List<SceneComponent>();
                _componentsByType.Add(t, comByType);
            }

            comByType.Add(component);
            return component;
        }

        public SceneComponent AddComponent(Type componentType)
        {
            Type baseType = typeof(SceneComponent);

            if (baseType.IsAssignableFrom(componentType) == false)
            {
                Engine.Log.Error($"Scene.AddObjectWithComponents: Attempt to add invalid component type {componentType.Name} to new object.");
                return null;
            }

            ConstructorInfo cInfo = componentType.GetConstructor(Type.EmptyTypes);
            if (cInfo == null)
            {
                Engine.Log.Error($"Scene.AddObjectWithComponents: Attempted to add valid component type {componentType.Name} to new object, but no parameterless-constructor was present.");
                return null;
            }
            else
            {
                SceneComponent component = cInfo.Invoke(ReflectionHelper.EmptyObjectArray) as SceneComponent;
                List<SceneComponent> comByType;
                if (!_componentsByType.TryGetValue(componentType, out comByType))
                {
                    comByType = new List<SceneComponent>();
                    _componentsByType.Add(componentType, comByType);
                }

                comByType.Add(component);

                if(_layer != null)
                    RegisterComponentOnLayer(component);

                return component;
            }
        }

        public void RemoveComponent<T>(T component) where T : SceneComponent, new()
        {
            if (component.Object != this)
                throw new Exception("Failed to remove component; It is owned by a different object.");

            _components.Remove(component);

            if (_layer != null)
                UnregisterComponentOnLayer(component);


            component.Destroy(this);
            _componentsByType[typeof(T)].Remove(component);
        }

        public void RemoveComponents<T>() where T : SceneComponent, new()
        {
            if (_componentsByType.TryGetValue(typeof(T), out List<SceneComponent> comByType))
            {
                comByType = new List<SceneComponent>();
                foreach (SceneComponent com in comByType)
                    _components.Remove(com);
            }
        }

        private void RegisterComponentOnLayer(SceneComponent sc)
        {
            if(sc is ICursorAcceptor ca)
                _layer.InputAcceptors.Add(ca);
        }

        private void UnregisterComponentOnLayer(SceneComponent sc)
        {
            if (sc is ICursorAcceptor ca)
                _layer.InputAcceptors.Remove(ca);
        }

        internal void Update(Timing time)
        {
            // Update own transform.
            _transform.Update(time);

            if ((_updateFlags & ObjectUpdateFlags.Self) == ObjectUpdateFlags.Self)
            {
                _components.For(0, 1, (index, component) =>
                {
                    if (component.IsEnabled)
                        component.OnUpdate(time);
                });
            }

            // Re-populate the local child list used when updating them.
            if ((_updateFlags & ObjectUpdateFlags.Children) == ObjectUpdateFlags.Children)
                _children.Objects.For(0, 1, (index, child) => child.Update(time));

            _transform.ResetFlags();
        }

        /// <summary>Gets or sets whether the object is rendered. The object will still be updated if <see cref="UpdateFlags"/> is true.</summary>
        public bool IsVisible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisibilityChanged?.Invoke(this, _visible);
                }
            }
        }

        /// <summary>Gets or sets how the object will be updated. The default set flags are: <see cref="ObjectUpdateFlags.Self"/> and <see cref="ObjectUpdateFlags.Children"/></summary>
        public ObjectUpdateFlags UpdateFlags
        {
            get => _updateFlags;
            set
            {
                if (_updateFlags != value)
                {
                    _updateFlags = value;
                    OnUpdateFlagsChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets the scene that the object is currently attached to, or null if not attached to any scenes.
        /// </summary>
        public Scene Scene => _scene;

        /// <summary>
        /// Gets the scene layer that the object is part of.
        /// </summary>
        public SceneLayer Layer
        {
            get => _layer;
            internal set
            {
                if (_layer != value)
                {
                    if (_scene != null)
                    {
                        OnRemovedFromScene?.Invoke(this, _scene, _layer);

                        for (int i = _components.Count - 1; i >= 0; i--)
                            UnregisterComponentOnLayer(_components[i]);
                    }

                    _layer = value;
                    if (value != null)
                    {
                        _scene = value.ParentScene;
                        OnAddedToScene?.Invoke(this, value.ParentScene, _layer);

                        for (int i = _components.Count - 1; i >= 0; i--)
                            RegisterComponentOnLayer(_components[i]);
                    }
                    else
                    {
                        _scene = null;
                    }

                    // TODO make this thread-safe.
                    for (int i = _children.Count - 1; i >= 0; i--)
                        _children[i].Layer = value;
                }
            }
        }

        /// <summary>
        /// Gets the object's <see cref="SceneObjectTransform"/>, which contains information about its position, rotation and scale and various helper properties.
        /// </summary>
        public SceneObjectTransform Transform => _transform;

        /// <summary>Gets an collection containing all of the child <see cref="SceneObject"/> instances attached to the current <see cref="SceneObject"/>.</summary>
        public SceneChildCollection Children => _children;

        /// <summary>Gets the object's parent <see cref="SceneObject"/>, if any. Value is null if the current object has no parent.</summary>
        public SceneObject Parent { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Engine"/> instance that the scene object is bound to.
        /// </summary>
        public Engine Engine => _engine;

        /// <summary>
        /// Gets or sets whether or not the current object should be updated.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
