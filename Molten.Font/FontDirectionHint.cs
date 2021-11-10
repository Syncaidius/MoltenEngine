namespace Molten.Font
{
    /// <summary>A hint to a font system on what direction gylphs should flow.<para/>
    /// Depreciated (Set to 2 in newer fonts).<para/>
    /// A neutral character has no inherent directionality; it is not a character with zero (0) width. Spaces and punctuation are examples of neutral characters.<para/> 
    /// Non-neutral characters are those with inherent directionality. For example, Roman letters (left-to-right) and Arabic letters (right-to-left) have directionality. 
    /// In a "normal" Roman font where spaces and punctuation are present, the font direction hints should be set to two (2).</summary>
    public enum FontDirectionHint : short
    {
        /// <summary>
        /// Like <see cref="StrongRightToLeft"/> but also contains neutrals
        /// </summary>
        StrongRightToLeftWithNeutrals = -2,

        /// <summary>
        /// Only strongly right to left
        /// </summary>
        StrongRightToLeft = -1,

        /// <summary>
        /// Fully mixed directional glyphs
        /// </summary>
        FullyMixed = 0,

        /// <summary>
        /// Only strongly left to right
        /// </summary>
        StrongLeftToRight = 1,

        /// <summary>
        /// Like <see cref="StrongLeftToRight"/> but also contains neutrals
        /// </summary>
        StrongLeftToRightWithNeutrals = 2,
    }
}
