namespace Molten.UI;

public class UIElementFilter
{
    List<Type> _filteredTypes;
    bool _allowDerivatives;

    internal UIElementFilter(Type[] acceptedTypes)
    {
        _filteredTypes = new List<Type>();

        if (acceptedTypes != null)
            _filteredTypes.AddRange(acceptedTypes);
    }

    /// <summary>
    /// Returns true if <typeparamref name="T"/> type is accepted by the current <see cref="UIElementFilter"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool IsAccepted<T>()
        where T : UIElement
    {
        return IsAccepted(typeof(T));
    }

    /// <summary>
    /// Returns true if the given <see cref="Type"/> is accepted by the current <see cref="UIElementFilter"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsAccepted(Type type)
    {
        if (_filteredTypes.Count > 0)
        {
            foreach (Type t in _filteredTypes)
            {
                if (t == type || t.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }

        return true;
    }

    public bool IsAccepted(UIElement e)
    {
        return IsAccepted(e.GetType());
    }

    /// <summary>
    /// Gets the filtered <see cref="Type"/> at the given index.
    /// </summary>
    /// <param name="index">The filter type index.</param>
    /// <returns></returns>
    public Type this[int index] => _filteredTypes[index];

    /// <summary>
    /// Gets the number of filtered types in the current <see cref="UIElementFilter"/>
    /// </summary>
    public int Count => _filteredTypes.Count;
}
