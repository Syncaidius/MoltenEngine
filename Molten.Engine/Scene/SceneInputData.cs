namespace Molten
{
    public delegate void SceneInputEventHandler<T>(SceneInputData<T> data) where T : struct;
    public delegate void SceneInputHandler<T>(T component) where T : IPointerReceiver;

    public struct SceneInputData<T> where T : struct
    {
        /// <summary>The value which describes the button or key that was pressed.</summary>
        public T InputValue;

        /// <summary>
        /// The object which invoked the event.
        /// </summary>
        public IPointerReceiver Object;

        /// <summary>
        /// If true, the object was dragged.
        /// </summary>
        public bool WasDragged;
    }
}
