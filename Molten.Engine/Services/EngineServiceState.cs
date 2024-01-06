namespace Molten;

public enum EngineServiceState
{
    /// <summary>
    /// The service has not been initialized.
    /// </summary>
    Uninitialized = 0,

    /// <summary>
    /// The service is initialized and ready to (re)start.
    /// </summary>
    Ready = 1,

    /// <summary>
    /// The service is starting.
    /// </summary>
    Starting = 2,

    /// <summary>
    /// The service is running.
    /// </summary>
    Running = 3,

    /// <summary>
    /// The service has been disposed.
    /// </summary>
    Disposed = 4,

    /// <summary>
    /// The service failed due to an error.
    /// </summary>
    Error = 10,
}
