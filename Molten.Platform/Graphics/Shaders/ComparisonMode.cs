namespace Molten.Graphics
{
    public enum ComparisonMode
    {
        /// <summary>Never pass the comparison.</summary>

        Never = 1,
        /// <summary>If the source data is less than the destination data, the comparison passes.</summary>

        Less = 2,

        /// <summary>If the source data is equal to the destination data, the comparison passes.</summary>

        Equal = 3,

        /// <summary>If the source data is less than or equal to the destination data, the comparison passes.</summary>

        LessEqual = 4,

        /// <summary>If the source data is greater than the destination data, the comparison passes.</summary>
        Greater = 5,

        /// <summary>If the source data is not equal to the destination data, the comparison passes.</summary>
        NotEqual = 6,

        /// <summary>If the source data is greater than or equal to the destination data, the comparison passes.</summary>

        GreaterEqual = 7,

        /// <summary>Always pass the comparison.</summary>
        Always = 8
    }
}
