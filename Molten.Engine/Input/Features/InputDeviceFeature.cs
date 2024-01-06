namespace Molten.Input;

public abstract class InputDeviceFeature
{
    public string Name { get; }

    public string Description { get; }

    public InputDeviceFeature(string name, string desc)
    {
        Name = name;
        Description = desc;
    }

    internal void Update(Timing time)
    {
        OnUpdate(time);
    }

    public abstract void ClearState();

    protected abstract void OnUpdate(Timing time);
}
