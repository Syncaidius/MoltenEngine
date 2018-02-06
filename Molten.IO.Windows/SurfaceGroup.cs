using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    internal class SurfaceGroup : IDisposable
    {
        Dictionary<Type, InputHandlerBase> _byType = new Dictionary<Type, InputHandlerBase>();
        List<InputHandlerBase> _handlers = new List<InputHandlerBase>();

        InputManager _manager;
        Logger _log;
        IWindowSurface _surface;

        internal SurfaceGroup(InputManager manager, Logger log, IWindowSurface surface)
        {
            _manager = manager;
            _log = log;
            _surface = surface;
        }

        internal T GetHandler<T>() where T : InputHandlerBase, new()
        {
            Type t = typeof(T);
            if (_byType.TryGetValue(t, out InputHandlerBase handler))
            {
                return handler as T;
            }
            else
            {
                handler = new T();
                handler.Initialize(_manager, _log, _surface);
                handler.OnDisposing += Handler_OnDisposing;
                _byType.Add(t, handler);
                _handlers.Add(handler);
                return handler as T;
            }
        }

        private void Handler_OnDisposing(EngineObject obj)
        {
            InputHandlerBase handler = obj as InputHandlerBase;
            _handlers.Remove(handler);
            _byType.Remove(obj.GetType());
        }

        internal void Update(Timing time)
        {
            foreach (InputHandlerBase handler in _handlers)
                handler.Update(time);
        }

        internal void ClearState()
        {
            foreach (InputHandlerBase handler in _handlers)
                handler.ClearState();
        }

        public void Dispose()
        {
            foreach (InputHandlerBase handler in _byType.Values)
                handler.Dispose();

            _byType.Clear();
            _log = null;
            _manager = null;
        }
    }
}
