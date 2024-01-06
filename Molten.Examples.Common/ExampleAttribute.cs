namespace Molten.Examples;

public class ExampleAttribute : Attribute
{
    public ExampleAttribute(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
