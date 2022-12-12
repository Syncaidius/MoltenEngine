using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal abstract class ComponentTypeTracker
    {
        internal abstract void Clear();

        internal abstract void Add(SceneComponent component);

        internal abstract void Remove(SceneComponent component);
    }

    internal class ComponentTypeTracker<T> : ComponentTypeTracker
        where T : class
    {
        static readonly string _strFormat = "Components: {0} -- Type: {1}";

        List<T> _components;
        IReadOnlyList<T> _readOnly;

        List<Action<T>> _addCallbacks;
        List<Action<T>> _removeCallbacks;

        internal ComponentTypeTracker()
        {
            _components = new List<T>();
            _readOnly = _components.AsReadOnly();
            _addCallbacks = new List<Action<T>>();
            _removeCallbacks = new List<Action<T>>();
        }

        internal override void Clear()
        {
            _addCallbacks.Clear();
            _removeCallbacks.Clear();
            _components.Clear();
        }

        internal void Hook(Action<T> add, Action<T> remove)
        {
            if(add != null)
                _addCallbacks.Add(add);

            if(remove != null)
                _removeCallbacks.Add(remove);
        }

        internal void Unhook(Action<T> add, Action<T> remove)
        {
            if (add != null)
                _addCallbacks.Remove(add);

            if (remove != null)
                _removeCallbacks.Remove(remove);
        }

        internal IReadOnlyList<T> GetObjects()
        {
            return _readOnly;
        }

        internal override void Add(SceneComponent component)
        {
            T cComp = component as T;
            _components.Add(cComp);
            foreach (Action<T> callback in _addCallbacks)
                callback(cComp);
        }

        internal override void Remove(SceneComponent component)
        {
            if (typeof(T).GetType().IsAssignableFrom(component.GetType()))
                throw new InvalidOperationException($"Cannot add object in SceneObjectTypeTracker for type '{typeof(T)}'. Does not implement or derive expected type");

            T cComp = component as T;
            _components.Remove(cComp);
            foreach (Action<T> callback in _removeCallbacks)
                callback(cComp);
        }

        public override string ToString()
        {
            return string.Format(_strFormat, _components.Count, typeof(T));
        }
    }
}
