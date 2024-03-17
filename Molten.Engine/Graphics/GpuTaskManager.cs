using Molten.Collections;
using System.Collections.Concurrent;

namespace Molten.Graphics;

public class GpuTaskManager : IDisposable
{
    Dictionary<GpuPriority, ThreadedQueue<GraphicsTask>> _tasks;
    ConcurrentDictionary<Type, ObjectPool<GraphicsTask>> _taskPool;
    GpuDevice _device;

    Dictionary<GpuPriority, GpuFrameBuffer<GpuCommandList>> _taskCmds;

    internal GpuTaskManager(GpuDevice parentDevice)
    {
        _device = parentDevice;
        _tasks = new Dictionary<GpuPriority, ThreadedQueue<GraphicsTask>>();
        GpuPriority[] priorities = Enum.GetValues<GpuPriority>();
        foreach (GpuPriority p in priorities)
        {
            _tasks[p] = new ThreadedQueue<GraphicsTask>();
            _taskCmds[p] = new GpuFrameBuffer<GpuCommandList>(parentDevice, (device) => device.Queue.GetCommandList());
        }

        _taskPool = new ConcurrentDictionary<Type, ObjectPool<GraphicsTask>>();
    }

    public T Get<T>() 
        where T : GraphicsTask, new()
    {
        if(!_taskPool.TryGetValue(typeof(T), out ObjectPool<GraphicsTask> pool))
        {
            pool = new ObjectPool<GraphicsTask>(() => new T());
            if(!_taskPool.TryAdd(typeof(T), pool))
            {
                _device.Log.Error($"Failed to create render task pool for '{typeof(T).Name}'");
                return new T();
            }
        }

        T task = (T)pool.GetInstance();
        task.Pool = pool;
        return task;
    }

    /// <summary>
    /// Pushes a <see cref="GraphicsTask"/> to the specified priority queue in the current <see cref="GpuTaskManager"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    /// <param name="task"></param>
    public void Push(GpuPriority priority, GraphicsTask task)
    {
        if (task.Validate())
        {
            ThreadedQueue<GraphicsTask> queue = _tasks[priority];
            queue.Enqueue(task);
        }
    }

    /// <summary>
    /// Queues a <see cref="GraphicsResourceTask{R}"/> on the current <see cref="GpuResource"/>.
    /// </summary>
    /// <param name="cmd">The command list that is pushing the task.</param>
    /// <param name="priority">The priority of the task.</param>
    /// <param name="resource">The <see cref="GpuResource"/>.</param>
    /// <param name="task">The <see cref="GraphicsResourceTask{R}"/> to be pushed.</param>
    public void Push<R, T>(GpuCommandList cmd, GpuPriority priority, R resource, T task)
        where R : GpuResource
        where T : GraphicsResourceTask<R>, new()
    {
        task.Resource = resource;
        switch (priority)
        {
            default:
            case GpuPriority.Immediate:
                if(task.Validate())
                    task.Process(cmd);
                break;

            case GpuPriority.StartOfFrame:
                Push(GpuPriority.StartOfFrame, task);
                break;

            case GpuPriority.EndOfFrame:
                Push(GpuPriority.EndOfFrame, task);
                break;
        }
    }

    /// <summary>
    /// Pushes a compute-based shader as a task.
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="shader">The compute shader to be run inside the task.</param>
    /// <param name="groupsX">The number of X compute thread groups.</param>
    /// <param name="groupsY">The number of Y compute thread groups.</param>
    /// <param name="groupsZ">The number of Z compute thread groups.</param>
    /// <param name="callback">A callback to run once the task is completed.</param>
    public void Push(GpuPriority priority, Shader shader, uint groupsX, uint groupsY, uint groupsZ, GraphicsTask.EventHandler callback = null)
    {
        Push(priority, shader, new Vector3UI(groupsX, groupsY, groupsZ), callback);
    }

    public void Push(GpuPriority priority, Shader shader, Vector3UI groups, GraphicsTask.EventHandler callback = null)
    {
        ComputeTask task = Get<ComputeTask>();
        task.Shader = shader;
        task.Groups = groups;
        task.OnCompleted += callback;
        Push(priority, task);
    }

    public void Dispose()
    {
        foreach (ObjectPool<GraphicsTask> pool in _taskPool.Values)
            pool.Dispose();

        foreach (GpuFrameBuffer<GpuCommandList> buffer in _taskCmds.Values)
            buffer.Dispose();

        _taskPool.Clear();
    }

    /// <summary>
    /// Processes all tasks held in the manager for the specified priority queue, for the current <see cref="GpuTaskManager"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    internal void Process(GpuPriority priority)
    {
        // TODO Implement "AllowBatching" property on RenderTask to allow multiple tasks to be processed in a single Begin()-End() command block
        //      Tasks that don't allow batching will:
        //       - Be executed in individual Begin()-End() command blocks
        //       - Be executed on the next available compute device queue
        //       - May not finish in the order they were requested due to task size, queue size and device performance.

        ThreadedQueue<GraphicsTask> queue = _tasks[priority];
        GpuCommandList cmd = _taskCmds[priority].Prepare();

        cmd.Begin();
        cmd.BeginEvent($"Process '{priority}' tasks");
        while (queue.TryDequeue(out GraphicsTask task))
            task.Process(cmd);

        cmd.EndEvent();
        cmd.End();
        _device.Queue.Execute(cmd);
    }
}
