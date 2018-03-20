using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// PCLT table - Printer Command Language Table
    /// The 'PCLT' table is strongly discouraged for OpenType™ fonts with TrueType outlines. 
    /// Extra information on this table can be found in the HP PCL 5 Printer Language Technical Reference Manual available from Hewlett-Packard Boise Printer Division.
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/pclt </summary>
    [FontTableTag("PCLT")]
    public class PCLT : MainFontTable
    {
        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        public PcltFontNumber FontNumber { get; private set; }

        public ushort Pitch { get; private set; }

        public ushort XHeight { get; private set; }

        public ushort Style { get; private set; }

        public ushort TypeFamily { get; private set; }

        public ushort CapHeight { get; private set; }

        public ushort SymbolSet { get; private set; }

        /// <summary>
        /// Gets a 16-byte ASCII string which appears in the “font print” of PCL printers. 
        /// Care should be taken to insure that the base string for all typefaces of a family are consistent, and that the designators for bold, italic, etc. are standardized.
        /// </summary>
        public string Typeface { get; private set; }

        public byte[] CharacterCompliment { get; private set; }

        /// <summary>
        /// Gets a 6-byte field which is composed of 3 parts. <para/>
        /// The first 3 bytes are an industry standard typeface family string. The fourth byte is a treatment character, such as R, B, I. 
        /// The last two characters are either zeroes for an unbound font or a two character mnemonic for a symbol set if symbol set found.
        /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/pclt#treatment-flags
        /// </summary>
        public string FileName { get; private set; }

        public byte StrokeWeight { get; private set; }

        public byte WidthType { get; private set; }

        public byte SerifStyle { get; private set; }

        /// <summary>
        /// Reserved for padding.
        /// </summary>
        public byte Reserved { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, TableHeader header, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            /*This 32-bit number is segmented in two parts. The most significant bit indicates native versus converted format. 
             * Only font vendors should create fonts with this bit zeroed. The 7 next most significant bits are assigned by Hewlett-Packard Boise Printer Division to major font vendors. 
             * The least significant 24 bits are assigned by the vendor. 
             * Font vendors should attempt to insure that each of their fonts are marked with unique values.*/
            FontNumber = new PcltFontNumber(reader.ReadUInt32());
            Pitch = reader.ReadUInt16();
            XHeight = reader.ReadUInt16();
            Style = reader.ReadUInt16();
            TypeFamily = reader.ReadUInt16(); // TODO convert this to readable flags/enum/string.
            CapHeight = reader.ReadUInt16();
            SymbolSet = reader.ReadUInt16(); // TODO convert this to readable flags/enum/string.
            byte[] typefaceBytes = reader.ReadBytes(16);
            Typeface = Encoding.ASCII.GetString(typefaceBytes).Replace("\0", ""); // TODO parse typeface flags. See: https://docs.microsoft.com/en-us/typography/opentype/spec/pclt#typeface
            CharacterCompliment = reader.ReadBytes(8); // TODO convert this to readable flags/enum/string. See: https://docs.microsoft.com/en-us/typography/opentype/spec/pclt#charactercomplement
            byte[] filenameBytes = reader.ReadBytes(6);
            FileName = Encoding.ASCII.GetString(filenameBytes).Replace("\0", ""); // TODO parse treatment flags.

            StrokeWeight = reader.ReadByte(); // TODO convert this to readable flags/enum/string.
            WidthType = reader.ReadByte(); // TODO convert this to readable flags/enum/string.
            SerifStyle = reader.ReadByte(); // TODO convert this to readable flags/enum/string.
            Reserved = reader.ReadByte();
        }
    }

    public class PcltFontNumber
    {
        public bool IsNativeVerusConvertedFormat { get; private set; }

        public PCLTVendor Vendor {get; private set;}

        public uint VendorFontID { get; private set; }

        internal PcltFontNumber(uint val)
        {
            IsNativeVerusConvertedFormat = (val >> 31) > 0;
            uint vendorCode = (val << 1) >> 25;
            Vendor = (PCLTVendor)vendorCode;
            VendorFontID = (val << 8) >> 8;
        }
    }

    public enum PCLTVendor : ushort
    {
        Unknown = 0,

        AdobeSystems = 'A',

        BitStreamInc = 'B',

        AgfaCorporation = 'C',

        BigelowAndHolmes = 'H',

        LinotypeCompany = 'L',

        MonotypeTypographyLtd = 'M',
    }
}
