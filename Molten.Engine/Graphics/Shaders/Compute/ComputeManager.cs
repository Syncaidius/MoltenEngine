using System.Collections.Concurrent;
using Molten.Collections;

namespace Molten.Graphics
{
    public class ComputeManager
    {
        ThreadedList<ComputeTask> _computeTasks = new ThreadedList<ComputeTask>();
        ConcurrentDictionary<string, ComputeTask> _computeByName = new ConcurrentDictionary<string, ComputeTask>();

        GraphicsDevice _device;

        internal ComputeManager(GraphicsDevice device)
        {
            _device = device;
        }

        public void AddTask(ComputeTask task)
        {
            _computeTasks.Add(task);
            _computeByName.TryAdd(task.Name.ToLower(), task);
        }

        public void Dispatch(ComputeTask task, uint x, uint y, uint z)
        {
            _device.Cmd.Dispatch(task, x, y, z);
        }

        public ComputeTask this[string name] => _computeByName[name.ToLower()];
    }
}
