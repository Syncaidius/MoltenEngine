namespace Molten.Font;

public class FontInfo
{
    public string FontFamily { get; private set; } = string.Empty;

    public string FontSubFamily { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public string LicenseDescription { get; private set; } = string.Empty;

    public string LicenseURL { get; private set; } = string.Empty;

    public string Designer { get; private set; } = string.Empty;

    public string DesignerURL { get; private set; } = string.Empty;

    public string Copyright { get; private set; } = string.Empty;

    public string Trademark { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    internal FontInfo(Name nameTable)
    {
        if (nameTable == null)
            return;

        FontPlatform plat = FontPlatform.Windows; // TODO Change this based on build platform.

        FontFamily = nameTable.GetNameString(FontNameType.FontFamilyName, plat);
        FontSubFamily = nameTable.GetNameString(FontNameType.FontSubFamilyName, plat);
        FullName = nameTable.GetNameString(FontNameType.FullFontName, plat);
        Version = nameTable.GetNameString(FontNameType.VersionString, plat);
        LicenseDescription = nameTable.GetNameString(FontNameType.LicenseDescription, plat);
        LicenseURL = nameTable.GetNameString(FontNameType.LicenseURL, plat);
        Designer = nameTable.GetNameString(FontNameType.Description, plat);
        DesignerURL = nameTable.GetNameString(FontNameType.DesignerUrl, plat);
        Copyright = nameTable.GetNameString(FontNameType.Copyright, plat);
        Trademark = nameTable.GetNameString(FontNameType.Trademark, plat);
        Description = nameTable.GetNameString(FontNameType.Description, plat);
    }
}
