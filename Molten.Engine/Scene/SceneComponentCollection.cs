using System.Reflection;

namespace Molten
{
    public class SceneComponentCollection : SceneCollection<SceneComponent>
    {
        Dictionary<Type, List<SceneComponent>> _componentsByType;
        Logger _log;

        internal SceneComponentCollection(Logger log, SceneObject parent) : base(parent)
        {
            _log = log;
            _componentsByType = new Dictionary<Type, List<SceneComponent>>();
        }

        public SceneComponent Add(Type componentType)
        {
            Type baseType = typeof(SceneComponent);

            if (baseType.IsAssignableFrom(componentType) == false)
            {
                _log.Error($"Scene.AddObjectWithComponents: Attempt to add invalid component type {componentType.Name} to new object.");
                return null;
            }

            ConstructorInfo cInfo = componentType.GetConstructor(Type.EmptyTypes);
            if (cInfo == null)
            {
                _log.Error($"Scene.AddObjectWithComponents: Attempted to add valid component type {componentType.Name} to new object, but no parameterless-constructor was present.");
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
                Add(component);

                if (Parent != null && Parent.Layer != null)
                    component.RegisterOnLayer();

                return component;
            }
        }

        public T Add<T>() where T : SceneComponent, new()
        {
            Type t = typeof(T);
            T component = Activator.CreateInstance(t) as T;

            List<SceneComponent> comByType;
            if (!_componentsByType.TryGetValue(t, out comByType))
            {
                comByType = new List<SceneComponent>();
                _componentsByType.Add(t, comByType);
            }

            comByType.Add(component);

            Add(component);

            return component;
        }

        public void Remove<T>(T component) where T : SceneComponent, new()
        {
            if (component.Object != Parent)
                throw new Exception("Failed to remove component; It is owned by a different object.");

            

            if (Parent.Layer != null)
                component.UnregisterFromLayer();

            _componentsByType[typeof(T)].Remove(component);
            
            Remove(component);
        }

        public void RemoveByType<T>() where T : SceneComponent, new()
        {
            if (_componentsByType.TryGetValue(typeof(T), out List<SceneComponent> comByType))
            {
                for(int i = comByType.Count - 1; i >=0; i--)
                    Objects.Remove(comByType[i]);
            }
        }
    }
}
