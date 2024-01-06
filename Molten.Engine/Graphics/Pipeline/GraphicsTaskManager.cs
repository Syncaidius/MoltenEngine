using Molten.Collections;
using System.Collections.Concurrent;

namespace Molten.Graphics;

public class GraphicsTaskManager : IDisposable
{
    Dictionary<RenderTaskPriority, ThreadedQueue<GraphicsTask>> _tasks;
    GraphicsDevice _device;

    ConcurrentDictionary<Type, ObjectPool<GraphicsTask>> _taskPool;

    internal GraphicsTaskManager(GraphicsDevice parentDevice)
    {
        _device = parentDevice;
        _tasks = new Dictionary<RenderTaskPriority, ThreadedQueue<GraphicsTask>>();
        RenderTaskPriority[] priorities = Enum.GetValues<RenderTaskPriority>();
        foreach (RenderTaskPriority p in priorities)
            _tasks[p] = new ThreadedQueue<GraphicsTask>();

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
    /// Pushes a <see cref="GraphicsTask"/> to the specified priority queue in the current <see cref="GraphicsTaskManager"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    /// <param name="task"></param>
    public void Push(RenderTaskPriority priority, GraphicsTask task)
    {
        ThreadedQueue<GraphicsTask> queue = _tasks[priority];
        queue.Enqueue(task);
    }

    /// <summary>
    /// Queues a <see cref="GraphicsResourceTask{R}"/> on the current <see cref="GraphicsResource"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    /// <param name="resource">The <see cref="GraphicsResource"/>.</param>
    /// <param name="task">The <see cref="GraphicsResourceTask{R}"/> to be pushed.</param>
    public void Push<R, T>(GraphicsPriority priority, R resource, T task)
        where R : GraphicsResource
        where T : GraphicsResourceTask<R>, new()
    {
        switch (priority)
        {
            default:
            case GraphicsPriority.Immediate:
                task.Process(_device.Renderer, _device);
                break;

            case GraphicsPriority.Apply:
                resource.ApplyQueue.Enqueue(task);
                break;

            case GraphicsPriority.StartOfFrame:
                Push(RenderTaskPriority.StartOfFrame, task);
                break;

            case GraphicsPriority.EndOfFrame:
                Push(RenderTaskPriority.EndOfFrame, task);
                break;
        }
    }

    public void Dispose()
    {
        foreach (ObjectPool<GraphicsTask> pool in _taskPool.Values)
            pool.Dispose();

        _taskPool.Clear();
    }

    /// <summary>
    /// Processes all tasks held in the manager for the specified priority queue, for the current <see cref="GraphicsTaskManager"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    internal void Process(RenderTaskPriority priority)
    {
        // TODO Implement "AllowBatching" property on RenderTask to allow multiple tasks to be processed in a single Begin()-End() command block
        //      Tasks that don't allow batching will:
        //       - Be executed in individual Begin()-End() command blocks
        //       - Be executed on the next available compute device queue
        //       - May not finish in the order they were requested due to task size, queue size and device performance.

        ThreadedQueue<GraphicsTask> queue = _tasks[priority];
        _device.Queue.Begin();
        _device.Queue.BeginEvent($"Process '{priority}' tasks");
        while (queue.TryDequeue(out GraphicsTask task))
            task.Process(_device.Renderer, _device);
        _device.Queue.EndEvent();
        _device.Queue.End();
    }
}
