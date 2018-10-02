using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void SceneObjectHandler(SceneObject obj);
    public delegate void SceneObjectVisibilityHandler(SceneObject obj, bool visible);
    public delegate void SceneObjectSceneHandler(SceneObject obj, Scene scene, SceneLayer layer);
    public delegate void SceneObjectLayerHandler(SceneObject obj, SceneLayer oldLayer, SceneLayer newLayer);

    public sealed class SceneObject : IUpdatable
    {
        Engine _engine;
        Scene _scene;
        SceneLayer _layer;

        // Transform-related variables
        SceneObjectTransform _transform;

        ObjectUpdateFlags _updateFlags;
        bool _visible;

        Dictionary<Type, List<SceneComponent>> _componentsByType;
        List<SceneComponent> _components;
        WatchableList<SceneObject> _children;
        List<SceneObject> _childrenToUpdate;
        bool _childrenDirty;

        public event SceneObjectVisibilityHandler OnVisibilityChanged;
        public event SceneObjectHandler OnUpdateFlagsChanged;

        public event SceneObjectSceneHandler OnAddedToScene;
        public event SceneObjectSceneHandler OnRemovedFromScene;

        public event SceneObjectLayerHandler OnLayerChanged;

        /// <summary>Creates a new instance of <see cref="SceneObject"/>.</summary>
        /// <param name="engine">The <see cref="Molten.Engine"/> instance that the object will be bound to.</param>
        /// <param name="updateFlags">The object's initial update flags.</param>
        /// <param name="visible">Decides whether the object is initially visible when spawned. The default value is true.</param>
        internal SceneObject(Engine engine, ObjectUpdateFlags updateFlags = ObjectUpdateFlags.Children | ObjectUpdateFlags.Self, bool visible = true)
        {
            _engine = engine;
            _components = new WatchableList<SceneComponent>();
            _componentsByType = new Dictionary<Type, List<SceneComponent>>();
            _children = new WatchableList<SceneObject>();
            _childrenToUpdate = new List<SceneObject>();
            _transform = new SceneObjectTransform(this);

            _children.OnItemAdded += _children_OnItemAdded;
            _children.OnItemRemoved += _children_OnItemRemoved;
            _children.OnRangeAdded += _children_OnRangeAdded;
            _children.OnRangeRemoved += _children_OnRangeRemoved;

            _updateFlags = updateFlags;
            IsVisible = visible;
        }

        private void _children_OnItemRemoved(WatchableList<SceneObject> list, SceneObject item)
        {
            item.Parent = null;
            _childrenDirty = true;
        }

        private void _children_OnItemAdded(WatchableList<SceneObject> list, SceneObject item)
        {
            item.Parent = this;
            _childrenDirty = true;
        }

        private void _children_OnRangeRemoved(WatchableList<SceneObject> list, IEnumerable<SceneObject> items, int itemsStartIndex)
        {
            _childrenDirty = true;
            foreach (SceneObject obj in items)
                obj.Parent = null;
        }

        private void _children_OnRangeAdded(WatchableList<SceneObject> list, IEnumerable<SceneObject> items, int itemsStartIndex)
        {
            foreach (SceneObject obj in items)
                obj.Parent = this;
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
                Engine.Log.WriteError($"Scene.AddObjectWithComponents: Attempt to add invalid component type {componentType.Name} to new object.");
                return null;
            }

            ConstructorInfo cInfo = componentType.GetConstructor(Type.EmptyTypes);
            if (cInfo == null)
            {
                Engine.Log.WriteError($"Scene.AddObjectWithComponents: Attempted to add valid component type {componentType.Name} to new object, but no parameterless-constructor was present.");
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
                return component;
            }
        }

        public void RemoveComponent<T>(T component) where T : SceneComponent, new()
        {
            if (component.Object != this)
                throw new Exception("Failed to remove component; It is owned by a different object.");

            _components.Remove(component);
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

        private void Update(Timing time)
        {
            // Update own transform.
            _transform.Update(time);

            if((_updateFlags & ObjectUpdateFlags.Self) == ObjectUpdateFlags.Self)
            {
                SceneComponent com = null;
                for(int i = 0; i < _components.Count; i++)
                {
                    com = _components[i];
                    if (com.IsEnabled)
                        com.OnUpdate(time);
                }
            }

            // Re-populate the local child list used when updating them.
            if ((_updateFlags & ObjectUpdateFlags.Children) == ObjectUpdateFlags.Children)
            {
                if (_childrenDirty)
                {
                    _childrenToUpdate.Clear();
                    _childrenToUpdate.AddRange(_children);
                    _childrenDirty = false;
                }

                // Update children.
                for (int i = 0; i < _childrenToUpdate.Count; i++)
                    _childrenToUpdate[i].Update(time);
            }

            _transform.ResetFlags();
        }

        void IUpdatable.Update(Timing time)
        {
            Update(time);
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
                if(_updateFlags != value)
                {
                    _updateFlags = value;
                    OnUpdateFlagsChanged?.Invoke(this);
                }
            }
        }

        /// <summary>Gets or [internally] sets the scene that the object is currently part of.</summary>
        public Scene Scene
        {
            get => _scene;
            internal set
            {
                if(_scene != value)
                {
                    if (_scene != null)
                        OnRemovedFromScene?.Invoke(this, _scene, _layer);

                    _scene = value;
                    _layer = _scene.DefaultLayer;

                    if (value != null)
                        OnAddedToScene?.Invoke(this, value, _layer);

                    // TODO make this thread-safe.
                    for (int i = _children.Count - 1; i >= 0; i--)
                        _children[i].Scene = value;
                }
            }
        }

        public SceneLayer Layer
        {
            get => _layer;
            internal set
            {
                if (Scene != null)
                {
                    // If the value is null, use the scene's default layer.
                    value = value ?? Scene.DefaultLayer;

                    if (value.ParentScene != Scene)
                        throw new SceneObjectException(Scene, this, "Cannot set object layer to the layer of a different scene. Must be a layer of the scene the object is current a member of.");

                    if(value != _layer)
                    {
                        SceneLayer oldLayer = _layer;
                        _layer = value;
                        OnLayerChanged?.Invoke(this, oldLayer, _layer);
                    }
                }
            }
        }

        Scene ISceneObject.Scene
        {
            get => _scene;
            set => this.Scene = value;
        }

        SceneLayer ISceneObject.Layer
        {
            get => _layer;
            set => this.Layer = value;
        }

        public SceneObjectTransform Transform => _transform;

        /// <summary>Gets an observable list of all child <see cref="SceneObject"/> instances.</summary>
        public WatchableList<SceneObject> Children => _children;

        /// <summary>Gets the </summary>
        public SceneObject Parent { get; internal set; }

        public Engine Engine => _engine;

        /// <summary>
        /// Gets or sets whether or not the current object should be updated.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
