namespace Molten
{
    public class ContentParameter
    {
        internal ContentParameter() { }

        public string Name { get; internal set; }

        public Type ExpectedType { get; internal set; }

        public object DefaultValue { get; internal set; }
    }
}
