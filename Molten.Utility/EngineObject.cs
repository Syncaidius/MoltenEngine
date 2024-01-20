using Molten.Utility;
using System.Runtime.Serialization;

namespace Molten;

/// <summary>
/// A helper base class for tracking and managing game objects. Provides a disposal structure and unique ID system.
/// </summary>
[DataContract]
public abstract class EngineObject : IDisposable
{
    [ThreadStatic]
    static uint _idCounter;

    /// <summary>
    /// Invoked when the current <see cref="EngineObject"/> is being disposed.
    /// </summary>
    public event MoltenEventHandler<EngineObject> OnDisposing;

    /// <summary>
    /// Creates a new instance of <see cref="EngineObject"/>.
    /// </summary>
    public EngineObject()
    {
        EOID = ((ulong)Thread.CurrentThread.ManagedThreadId << 32) | _idCounter++;
        Name = $"EO {EOID} - {this.GetType().Name}";
    }

    public override string ToString()
    {
        return Name;
    }

    public void Dispose()
    {
        Dispose(false);
    }

    /// <summary>Disposes of the current <see cref="EngineObject"/> instance.</summary>
    /// <param name="immediate">If true, the object should dispose immediately 
    /// and avoid any deferring the release of any native resources.</param>
    public void Dispose(bool immediate)
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        Interlocked.CompareExchange(ref OnDisposing, null, null)?.Invoke(this);
        OnDispose(immediate);
    }

    /// <summary>Invoked when <see cref="Dispose"/> is called.</summary>
    /// <param name="immediate">If true, the object should dispose immediately 
    /// and avoid any deferring the release of any native resources.</param>
    protected abstract void OnDispose(bool immediate);

    /// <summary>Gets whether or not the object has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the unique <see cref="EngineObject"/> ID (EOID) of the current <see cref="EngineObject"/>.
    /// </summary>
    public ulong EOID { get; }

    /// <summary>
    /// Gets the name of the object. Multiple <see cref="EngineObject"/> can have the same name.
    /// </summary>
    public virtual string Name { get; set; }
}
