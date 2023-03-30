namespace Molten.Graphics
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShaderNodeAttribute : Attribute
    {
        public ShaderNodeAttribute(ShaderNodeParseType parseType)
        {
            ParseType = parseType;
        }

        public ShaderNodeParseType ParseType { get; }
    }

    public enum ShaderNodeParseType
    {
        Enum = 0,

        Byte = 1,

        Int32 = 2,

        UInt32 = 3,

        Float = 4,

        Bool = 5,

        String = 6,

        Object = 7,

        /// <summary>
        /// RGBA color. For example "255 255 255 255" for white.
        /// </summary>
        Color = 8,
    }
}
