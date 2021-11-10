namespace Molten
{
    public delegate void SceneInputEventHandler<T>(SceneEventData<T> data) where T : struct;
    public delegate void SceneInputHandler<T>(T component) where T : ICursorAcceptor;

    public struct SceneEventData<T> where T : struct
    {
        public Vector2F Position;

        /// <summary>The movement delta. For the mouse scroll wheel, this is stored the Y axis.</summary>
        public Vector2F Delta;

        /// <summary>The value which describes the button or key that was pressed.</summary>
        public T InputValue;

        /// <summary>
        /// The object which invoked the event.
        /// </summary>
        public ICursorAcceptor Object;

        /// <summary>
        /// If true, the object was dragged.
        /// </summary>
        public bool WasDragged;
    }
}
