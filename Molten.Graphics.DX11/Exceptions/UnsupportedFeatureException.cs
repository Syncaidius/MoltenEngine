namespace Molten
{
    public class UnsupportedFeatureException : Exception
    {
        public UnsupportedFeatureException(string featureName) : base($"{featureName} is not supported.") { }
    }
}
