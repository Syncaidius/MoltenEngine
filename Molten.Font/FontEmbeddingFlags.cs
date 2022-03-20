namespace Molten.Font
{
    /// <summary>
    /// Indicates font embedding licensing rights for the font. Embeddable fonts may be stored in a document.
    /// </summary>
    [Flags]
    public enum FontEmbeddingFlags : ushort
    {
        /// <summary>
        /// Installable Embedding: No fsType bit is set. Thus fsType is zero. Fonts with this setting indicate that they may be embedded and permanently installed on the remote system by an application. <para/>
        /// The user of the remote system acquires the identical rights, obligations and licenses for that font as the original purchaser of the font, and is subject to the same end-user license agreement, copyright, design patent, and/or trademark as was the original purchaser.
        /// </summary>
        InstabllableEmbedding = 0,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved = 1,

        /// <summary>
        /// Restricted License embedding: Fonts that have **only** this bit set **must not be modified, embedded or exchanged in any manner** without first obtaining permission of the legal owner. <para/>
        /// CAUTION: For Restricted License embedding to take effect, it must be the only level of embedding selected.
        /// </summary>
        RestrictedLicenseEmbedding = 2,

        /// <summary>
        /// Preview & Print embedding: When this bit is set, the font may be embedded, and temporarily loaded on the remote system. <para/>
        /// Documents containing Preview & Print fonts must be opened “read-only;” no edits can be applied to the document.
        /// </summary>
        PreviewPrintEmbedding = 4,

        /// <summary>
        /// Editable embedding: When this bit is set, the font may be embedded but must only be installed **temporarily** on other systems. <para/>
        /// In contrast to Preview & Print fonts, documents containing Editable fonts _may_ be opened for reading, editing is permitted, and changes may be saved.
        /// </summary>
        EditableEmbedding = 8,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved4 = 16,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved5 = 32,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved6 = 64,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved7 = 128,

        /// <summary>
        /// No subsetting: When this bit is set, the font may not be subsetted prior to embedding. Other embedding restrictions specified in bits 0-3 and 9 also apply.
        /// </summary>
        NoSubsetting = 256,

        /// <summary>
        /// Bitmap embedding only: When this bit is set, only bitmaps contained in the font may be embedded. No outline data may be embedded. <para/>
        /// If there are no bitmaps available in the font, then the font is considered unembeddable and the embedding services will fail. <para/>
        /// Other embedding restrictions specified in bits 0-3 and 8 also apply.
        /// </summary>
        BitmapEmbeddingOnly = 512,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved10 = 1024,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved11 = 2048,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved12 = 8192,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved14 = 16384,

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        Reserved15 = 32768,
    }
}
