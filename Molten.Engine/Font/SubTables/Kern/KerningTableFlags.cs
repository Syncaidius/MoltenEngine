namespace Molten.Font;

public enum KerningTableFlags : byte
{
    None = 0,

    Horizonal = 1,

    Minimum = 2,

    /// <summary>
    /// If set to 1, kerning is perpendicular to the flow of the text. <para/>
    /// If the text is normally written horizontally, kerning will be done in the up and down directions. <para/>
    /// If kerning values are positive, the text will be kerned upwards; if they are negative, the text will be kerned downwards. If the text is normally written vertically, kerning will be done in the left and right directions. If kerning values are positive, the text will be kerned to the right; if they are negative, the text will be kerned to the left. <para/>
    /// The value 0x8000 in the kerning data resets the cross-stream kerning back to 0.
    /// </summary>
    CrossStream = 4,

    Override = 8,

    Reserved4 = 16,

    Reserved5 = 32,

    Reserved6 = 64,

    Reserved7 = 128,
}
