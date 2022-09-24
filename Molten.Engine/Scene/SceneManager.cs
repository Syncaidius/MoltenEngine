using Molten.Collections;
using Molten.Input;

namespace Molten
{
    internal class SceneManager : IDisposable
    {
        List<Scene> _scenes;
        ThreadedQueue<SceneChange> _pendingChanges;

        internal SceneManager()
        {
            _scenes = new List<Scene>();
            _pendingChanges = new ThreadedQueue<SceneChange>();
        }


        internal void QueueChange(Scene scene, SceneChange change)
        {
            change.Scene = scene;
            _pendingChanges.Enqueue(change);
        }
        internal void Add(Scene scene)
        {
            _scenes.Add(scene);
        }

        internal void Remove(Scene scene)
        {
            _scenes.Remove(scene);
        }

        public void Dispose()
        {
            // Dispose of scenes
            for (int i = 0; i < _scenes.Count; i++)
                _scenes[i].Dispose();

            _scenes.Clear();
        }

        internal void Update(Timing time)
        {
            while (_pendingChanges.TryDequeue(out SceneChange change))
                change.Process();

            // Run through all the scenes and update if enabled.
            foreach (Scene scene in _scenes)
            {
                if (scene.IsEnabled)
                    scene.Update(time);
            }
        }
    }
}
