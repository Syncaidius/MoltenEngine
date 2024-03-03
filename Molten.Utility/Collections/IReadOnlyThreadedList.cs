namespace Molten.Collections;

/// <summary>
/// Represents an implementation of a read-only threaded list.
/// </summary>
/// <typeparam name="T">The type of objects stored in the read-only list.</typeparam>
public interface IReadOnlyThreadedList<out T> : IReadOnlyList<T>
{
    /// <summary>
    /// Gets the capacity of the current <see cref="IReadOnlyThreadedList{T}"/>.
    /// </summary>
    int Capacity { get; }

    /// <summary>Gets the number of items in the current <see cref="IReadOnlyThreadedList{T}"/>, without using any thread-safety.</summary>
    int UnsafeCount { get; }

    /// <summary>Copies the contents of the list to an array and returns it.</summary>
    /// <returns>An array containing the list contents.</returns>
    T[] ToArray();

    /// <summary>Runs a for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void For(int start, Action<int, T> callback);

    /// <summary>Runs a for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void For(int start, Func<int, T, bool> callback);

    /// <summary>Runs a for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="end">The element to iterate up to.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void For(int start, int increment, int end, Action<int, T> callback);

    /// <summary>Runs a for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="end">The element to iterate up to.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void For(int start, int increment, int end, Func<int, T, bool> callback);

    /// <summary>Runs a reversed for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="decrement">The decremental value.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void ForReverse(int decrement, Action<int, T> callback);

    /// <summary>Runs a reversed for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="decrement">The decremental value.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void ForReverse(int start, int decrement, Action<int, T> callback);

    /// <summary>Runs a reversed for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="decrement">The decremental value.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void ForReverse(int decrement, Func<int, T, bool> callback);

    /// <summary>Runs a reversed for loop inside an interlock on the current <see cref="IReadOnlyThreadedList{T}"/> instance. This allows the collection to be iterated over in a thread-safe manner. 
    /// However, it can hurt performance if the loop takes too long to execute while other threads are waiting to access the list. <para/>
    /// Return true from the callback to break out of the for loop.</summary>
    /// <param name="start">The start index.</param>
    /// <param name="decrement">The decremental value.</param>
    /// <param name="callback">The callback to run on each iteration. The callback can optionally return true to break out of the loop.</param>
    void ForReverse(int start, int decrement, Func<int, T, bool> callback);
}
