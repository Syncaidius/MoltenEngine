namespace Molten.Font
{
    public enum FontNameType : ushort
    {
        /// <summary>
        /// Copyright notice.
        /// </summary>
        Copyright = 0,

        /// <summary>
        /// Font Family name. <para/>
        /// This family name is assumed to be shared among fonts that differ only in weight or style (italic, oblique). Font Family name is used in combination with Font Subfamily name (name ID 2). Some applications that use this pair of names assume that a Font Family name is shared by at most four fonts that form a font style-linking group: regular, italic, bold, and bold italic. To be compatible with the broadest range of platforms and applications, fonts should limit use of any given Font Family name in this manner. (This four-way distinction should also be reflected in [OS/2.fsSelection](os2.htm#fss) bit settings.) For fonts within an extended typographic family that fall outside this four-way distinction, the distinguishing attributes should be reflected in the Font Family name so that those fonts appear as a separate font family. For example, the Font Family name for the Arial Narrow font is “Arial Narrow”; the Font Family name for the Arial Black font is “Arial Black”. (Note that, in such cases, name ID 16 should also be included with a shared name that reflects the full, typographic family.)
        /// </summary>
        FontFamilyName = 1,

        /// <summary>
        ///  Font Subfamily name. <para/>
        ///  The Font Subfamily name distinguishes the fonts in a group with the same Font Family name (name ID 1). This is assumed to address style (italic, oblique) and weight variants only. A font with no distinctive weight or style (e.g. medium weight, not italic, and OS/2.fsSelection bit 6 set) should use the string “Regular” as the Font Subfamily name (for English language). Font Subfamily name is used in combination with Font Family name (name ID 1). Some applications that use this pair of names assume that a Font Family name is shared by at most four fonts that form a font style-linking group. These four fonts may have Subfamily name values that reflect various weights or styles, with four-way “Bold” and “Italic” style-linking relationships indicated using OS/2.fsSelection bits 0, 5 and 6. Within an extended typographic family that includes fonts beyond regular, bold, italic, or bold italic, distinctions are made in the Font Family name, so that fonts appear to be in separate families. In some cases, this may lead to specifying a Subfamily name of “Regular” for a font that might not otherwise be considered a _regular_ font. For example, the Arial Black font has a Font Family name of “Arial Black” and a Subfamily name of “Regular”. (Note that, in such cases, name IDs 16 and 17 should also be included, using a shared value for name ID 16 that reflects the full typographic family, and values for name ID 17 that appropriately reflect the actual design variant of each font.)
        /// </summary>
        FontSubFamilyName = 2,

        /// <summary>
        /// Unique font identifier
        /// </summary>
        UniqueFontIdentifier = 3,

        /// <summary>
        /// Full font name that reflects all family and relevant subfamily descriptors. <para/> 
        /// The full font name is generally a combination of name IDs 1 and 2, or of name IDs 16 and 17, or a similar human-readable variant. For fonts in extended typographic families (that is, families that include more than regular, italic, bold, and bold italic variants), values for name IDs 1 and 2 are normally chosen to provide compatibility with certain applications that assume a family has at most four style-linked fonts. In that case, some fonts may end up with a Subfamily name (name ID 2) of “Regular” even though the font would not be considered, typographically, a _regular_ font. For such non-regular fonts in which name ID 2 is specified as “Regular”, the “Regular” descriptor would generally be omitted from name ID 4. For example, the Arial Black font has a Font Family name (name ID 1) of “Arial Black” and a Subfamily name (name ID 2) of “Regular”, but has a full font name (name ID 4) of “Arial Black”. Note that name IDs 16 and 17 should also be included in these fonts, and that name ID 4 would typically be a combination of name IDs 16 and 17, without needing any additional qualifications regarding “Regular”.
        /// </summary>
        FullFontName = 4,

        /// <summary>
        /// Version string.  <para/>
        /// Should begin with the syntax 'Version .' (upper case, lower case, or mixed, with a space between “Version” and the number). The string must contain a version number of the following form: one or more digits (0-9) of value less than 65,535, followed by a period, followed by one or more digits of value less than 65,535. Any character other than a digit will terminate the minor number.  <para/>
        /// A character such as “;” is helpful to separate different pieces of version information. The first such match in the string can be used by installation software to compare font versions. Note that some installers may require the string to start with “Version ”, followed by a version number as above.
        /// </summary>
        VersionString = 5,

        /// <summary>
        /// PostScript name for the font. <para/>
        /// Name ID 6 specifies a string which is used to invoke a PostScript language font that corresponds to this OpenType font. In a CFF OpenType font, there is no requirement that this name be the same as the font name in the CFF’s Name INDEX. Thus, the same CFF may be shared among multiple font components in a Font Collection.
        /// </summary>
        PostScriptFontName = 6,

        /// <summary>
        /// Trademark. <para/>
        ///  this is used to save any trademark notice/information for this font. Such information should be based on legal advice. This is _distinctly_ separate from the copyright.
        /// </summary>
        Trademark = 7,

        /// <summary>
        /// Manufacturer Name.
        /// </summary>
        ManufacturerName = 8,

        /// <summary>
        /// Designer; name of the designer of the typeface.
        /// </summary>
        Designer = 9,

        /// <summary>
        /// Description of the typeface. Can contain revision information, usage recommendations, history, features, etc.
        /// </summary>
        Description = 10,

        /// <summary>
        /// URL of font vendor (with protocol, e.g., http://, ftp://). If a unique serial number is embedded in the URL, it can be used to register the font.
        /// </summary>
        VendorUrl = 11,

        /// <summary>
        /// URL of typeface designer (with protocol, e.g., http://, ftp://).
        /// </summary>
        DesignerUrl = 12,

        /// <summary>
        /// Description of how the font may be legally used, or different example scenarios for licensed use. This field should be written in plain language, not legalese.
        /// </summary>
        LicenseDescription = 13,

        /// <summary>
        /// URL where additional licensing information can be found.
        /// </summary>
        LicenseURL = 14,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved15 = 15,

        /// <summary>
        /// Typographic Family name. <para/>
        /// The typographic family grouping doesn't impose any constraints on the number of faces within it, in contrast with the 4-style family grouping (ID 1), which is present both for historical reasons and to express style linking groups.  <para/>
        /// If name ID 16 is absent, then name ID 1 is considered to be the typographic family name. (In earlier versions of the specification, name ID 16 was known as "Preferred Family".)
        /// </summary>
        TypographicFamilyName = 16,

        /// <summary>
        /// Typographic Subfamily name. <para/>
        ///  This allows font designers to specify a subfamily name within the typographic family grouping. This string must be unique within a particular typographic family. If it is absent, then name ID 2 is considered to be the typographic subfamily name.  <para/>
        /// (In earlier versions of the specification, name ID 17 was known as "Preferred Subfamily".)
        /// </summary>
        TypographicSubFamilyName = 17,

        /// <summary>
        /// Compatible Full (Macintosh only). <para/>
        ///  On the Macintosh, the menu name is constructed using the FOND resource. This usually matches the Full Name. If you want the name of the font to appear differently than the Full Name, you can insert the Compatible Full Name in ID 18.
        /// </summary>
        CompatibleFull = 18,

        /// <summary>
        /// Sample text. <para/>
        ///  This can be the font name, or any other text that the designer thinks is the best sample to display the font in.
        /// </summary>
        SampleText = 19,

        /// <summary>
        /// PostScript CID findfont name.  <para/>
        /// Its presence in a font means that the nameID 6 holds a PostScript font name that is meant to be used with the “composefont” invocation in order to invoke the font in a PostScript interpreter. <para/>
        /// See the definition of name ID 6. The value held in the name ID 20 string is interpreted as a PostScript font name that is meant to be used with the “findfont” invocation, in order to invoke the font in a PostScript interpreter.
        /// </summary>
        PostScriptCID = 20,

        /// <summary>
        /// WWS Family Name.  <para/>
        /// Used to provide a WWS-conformant family name in case the entries for IDs 16 and 17 do not conform to the WWS model. (That is, in case the entry for ID 17 includes qualifiers for some attribute other than weight, width or slope.)  <para/>
        /// If bit 8 of the fsSelection field is set, a WWS Family Name entry should not be needed and should not be included. Conversely, if an entry for this ID is included, bit 8 should not be set.  <para/>
        /// (See OS/2 'fsSelection' field for details.) Examples of name ID 21: “Minion Pro Caption” and “Minion Pro Display”. (Name ID 16 would be “Minion Pro” for these examples.)
        /// </summary>
        WWSFamilyName = 21,

        /// <summary>
        /// WWS Subfamily Name.  <para/>
        /// Used in conjunction with ID 21, this ID provides a WWS-conformant subfamily name (reflecting only weight, width and slope attributes) in case the entries for IDs 16 and 17 do not conform to the WWS model. <para/>
        ///  As in the case of ID 21, use of this ID should correlate inversely with bit 8 of the fsSelection field being set. Examples of name ID 22: “Semibold Italic”, “Bold Condensed”. (Name ID 17 could be “Semibold Italic Caption”, or “Bold Condensed Display”, for example.) 
        /// </summary>
        WWSSubFamilyName = 22,

        /// <summary>
        /// Light Background Palette. <para/>
        /// This ID, if used in the CPAL table’s Palette Labels Array, specifies that the corresponding color palette in the CPAL table is appropriate to use with the font when displaying it on a light background such as white.  <para/>
        /// Name table strings for this ID specify the user interface strings associated with this palette. 
        /// </summary>
        LightBackgroundPalette = 23,

        /// <summary>
        /// Dark Background Palette.  <para/>
        /// This ID, if used in the CPAL table’s Palette Labels Array, specifies that the corresponding color palette in the CPAL table is appropriate to use with the font when displaying it on a dark background such as black.  <para/>
        /// Name table strings for this ID specify the user interface strings associated with this palette 
        /// </summary>
        DarkBackgroundPalette = 24,

        /// <summary>
        /// Variations PostScript Name Prefix.  <para/>
        /// If present in a variable font, it may be used as the family prefix in the PostScript Name Generation for Variation Fonts algorithm. The character set is restricted to ASCII-range uppercase Latin letters, lowercase Latin letters, and digits. All name strings for name ID 25 within a font, when converted to ASCII, must be identical.  <para/>
        /// See: http://wwwimages.adobe.com/content/dam/Adobe/en/devnet/font/pdfs/5902.AdobePSNameGeneration.html for reasons to include name ID 25 in a font, and for examples.
        /// </summary>
        VariationsPostScriptNamePrefix = 25,
    }
}
