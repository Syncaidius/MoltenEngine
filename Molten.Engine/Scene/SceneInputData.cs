namespace Molten;

public delegate void SceneInputEventHandler<T>(SceneInputData<T> data) where T : struct;

public struct SceneInputData<T> where T : struct
{
    /// <summary>The value which describes the button or key that was pressed.</summary>
    public T InputValue;

    /// <summary>
    /// The object which invoked the event.
    /// </summary>
    public SceneComponent Component;

    /// <summary>
    /// If true, the object was dragged.
    /// </summary>
    public bool WasDragged;
}
