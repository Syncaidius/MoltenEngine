using Molten.Collections;
using System.Collections.Concurrent;

namespace Molten.Graphics;

public class GraphicsTaskManager : IDisposable
{
    Dictionary<GraphicsTaskPriority, ThreadedQueue<GraphicsTask>> _tasks;
    ConcurrentDictionary<Type, ObjectPool<GraphicsTask>> _taskPool;
    GraphicsDevice _device;

    internal GraphicsTaskManager(GraphicsDevice parentDevice)
    {
        _device = parentDevice;
        _tasks = new Dictionary<GraphicsTaskPriority, ThreadedQueue<GraphicsTask>>();
        GraphicsTaskPriority[] priorities = Enum.GetValues<GraphicsTaskPriority>();
        foreach (GraphicsTaskPriority p in priorities)
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
    public void Push(GraphicsTaskPriority priority, GraphicsTask task)
    {
        if (task.Validate())
        {
            ThreadedQueue<GraphicsTask> queue = _tasks[priority];
            queue.Enqueue(task);
        }
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
        task.Resource = resource;
        switch (priority)
        {
            default:
            case GraphicsPriority.Immediate:
                if(task.Validate())
                    task.Process(_device.Queue);
                break;

            case GraphicsPriority.Apply:
                if(task.Validate())
                    resource.ApplyQueue.Enqueue(task);
                break;

            case GraphicsPriority.StartOfFrame:
                Push(GraphicsTaskPriority.StartOfFrame, task);
                break;

            case GraphicsPriority.EndOfFrame:
                Push(GraphicsTaskPriority.EndOfFrame, task);
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
    public void Push(GraphicsTaskPriority priority, HlslShader shader, uint groupsX, uint groupsY, uint groupsZ, GraphicsTask.EventHandler callback = null)
    {
        Push(priority, shader, new Vector3UI(groupsX, groupsY, groupsZ), callback);
    }

    public void Push(GraphicsTaskPriority priority, HlslShader shader, Vector3UI groups, GraphicsTask.EventHandler callback = null)
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

        _taskPool.Clear();
    }

    /// <summary>
    /// Processes all tasks held in the manager for the specified priority queue, for the current <see cref="GraphicsTaskManager"/>.
    /// </summary>
    /// <param name="priority">The priority of the task.</param>
    internal void Process(GraphicsTaskPriority priority)
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
            task.Process(_device.Queue);

        _device.Queue.EndEvent();
        _device.Queue.End();
    }
}
