using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ComputeManager : IComputeManager
    {
        ThreadedList<ComputeTask> _computeTasks = new ThreadedList<ComputeTask>();
        ThreadedDictionary<string, ComputeTask> _computeByName = new ThreadedDictionary<string, ComputeTask>();

        Device _device;

        internal ComputeManager(Device device)
        {
            _device = device;
        }

        internal void AddTask(ComputeTask task)
        {
            _computeTasks.Add(task);
            _computeByName.Add(task.Name.ToLower(), task);
        }

        public void Dispatch(IComputeTask task, uint x, uint y, uint z)
        {
            _device.Dispatch(task as ComputeTask, x, y, z);
        }

        public IComputeTask this[string name] => _computeByName[name.ToLower()];
    }
}
