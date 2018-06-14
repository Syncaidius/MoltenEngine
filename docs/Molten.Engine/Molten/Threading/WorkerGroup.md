  
# Molten.Threading.WorkerGroup
A group of worker threads that process tasks stored on a shared queue. This is great for chewing through a queue full of one-time tasks which don't need to run
            inside an engine loop, such as AI pathfinding tasks or AI logic, asset loading, etc.
  
*  [Clear](docs/Molten.Engine/Molten/Threading/WorkerGroup/Clear.md)  
*  [SetTaskQueue(Molten.Collections.ThreadedQueue{Molten.Threading.IWorkerTask})](docs/Molten.Engine/Molten/Threading/WorkerGroup/SetTaskQueue.md)  
*  [WorkerCount](docs/Molten.Engine/Molten/Threading/WorkerGroup/WorkerCount.md)