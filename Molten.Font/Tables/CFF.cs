using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>CFF — Compact Font Format table.<para/>
    /// See: http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf </summary>
    [FontTableTag("CFF", "maxp")]
    public class CFF : FontTable
    {
        const uint STANDARD_ID_COUNT = 390; // See Appendix A -- SID/Name count (Standard ID).

        const uint MAX_SUBR_NESTING = 10;

        public byte MajorVersion { get; private set; }

        public byte MinorVersion { get; private set; }

        public string FontName { get; private set; }


        List<CFFIndexTable> _charStrings = new List<CFFIndexTable>();

        /// <summary>
        /// A list of Local Subrs associated with FDArrays. Can be empty.
        /// </summary>
        List<CFFIndexTable> _localSubrsPerFont = new List<CFFIndexTable>();

        /// <summary>
        /// // A Local Subrs associated with Top DICT. Can be NULL.
        /// </summary>
        CFFIndexTable _localSubrs;

        int _fontDictLength;
        Dictionary<ushort, byte> _fdSelect = new Dictionary<ushort, byte>();

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Maxp maxp = dependencies.Get<Maxp>();
            ushort numGlyphs = maxp.NumGlyphs;

            // Read header
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();

            byte headerSize = reader.ReadByte();
            byte offSize = reader.ReadByte();


            if (MajorVersion == 1 && MinorVersion == 0)
            {
                CFFIndexTable nameIndex = new CFFIndexTable(reader, log, this, headerSize);
                CFFIndexTable topDictIndex = new CFFIndexTable(reader, log, this, nameIndex.OffsetToNextBlock);
                CFFIndexTable stringIndex = new CFFIndexTable(reader, log, this, topDictIndex.OffsetToNextBlock);
                CFFIndexTable globalSubStrIndex = new CFFIndexTable(reader, log, this, stringIndex.OffsetToNextBlock);

                uint sidMax = (uint)stringIndex.Length + STANDARD_ID_COUNT;
                ParseNameData(reader, nameIndex);
                ParseDictData(reader, log, topDictIndex, DictDataType.TopLevel, numGlyphs, sidMax);

                // Check if all fdIndex in FDSelect are valid
                foreach(byte b in _fdSelect.Values)
                {
                    if (b >= _fontDictLength)
                        return; // TODO failure
                }

                // Check if all charstrings (font hinting code for each glyph) are valid.
                for(int i = 0; i < _charStrings.Count; i++)
                {
                    if (!ValidateType2CharStringIndex(reader, _charStrings[i], globalSubStrIndex))
                        return; // TODO failure - $"failed validating charstring set {i}";
                }
            }
            else
            {
                log.WriteDebugLine($"[CFF] Unsupported CFF version {MajorVersion}.{MinorVersion}");
            }
        }

        private bool ValidateType2CharStringIndex(EnhancedBinaryReader reader, CFFIndexTable charStringIndex, CFFIndexTable globalSubStrIndex)
        {
            for(ushort g = 0; g < charStringIndex.Length; g++)
            {
                if (charStringIndex.Objects[g].DataSize > ushort.MaxValue) // Max char string length
                    return false; // TODO failure
                // Get a local subrs for the glyph
                CFFIndexTable localSubrsToUse = null;
                if (!SelectLocalSubr(out localSubrsToUse, g))
                    return false; // TODO failure

                // If localSubrsTouse is still null, use an empty one.
                localSubrsToUse = new CFFIndexTable(reader, this);


                // Check a charstring for the [i]-th glyph
                Stack<uint> argStack = new Stack<uint>();
                bool foundEndChar = false;
                bool foundWidth = false;
                uint numStems = 0;
                if (!ExecuteType2CharString(reader, 0, globalSubStrIndex, localSubrsToUse, charStringIndex.Objects[g], argStack, out foundEndChar, out foundWidth, out numStems))
                    return false;  // TODO failure

                if (!foundEndChar)
                    return false; // TODO failure
            }

            return true;
        }

        private bool ExecuteType2CharString(EnhancedBinaryReader reader, uint callDepth, CFFIndexTable globalSubStrIndex, 
            CFFIndexTable localSubrsTouse, CFFIndexTable.ObjectEntry charString, Stack<uint> argStack, 
            out bool foundEndChar, out bool foundWidth, out uint numStems)
        {
            foundEndChar = false;
            foundWidth = false;
            numStems = 0;

            if (callDepth > MAX_SUBR_NESTING)
                return false; // TODO failure - exceeded maximum nesting

            uint length = charString.DataSize;
            uint curPos = GetLocalOffset(reader);
            uint endPos = charString.Offset + length;
            SetLocalOffset(reader, charString.Offset);
            SetLocalOffset(reader, curPos);


            while (GetLocalOffset(reader) < endPos)
            {
                uint operatorOrOperand = 0;
                bool isOperator = false;
                if (!ReadNextNumberFromType2CharString(reader, out operatorOrOperand, out isOperator))
                    return false; // TODO failure

#if DUMP_T2CHARSTRING
                /*
                  You can dump all operators and operands (except mask bytes for hintmask
                  and cntrmask) by the following code:
                */

                if(!isOperator){

#endif
            }

            return true;
        }

        private bool ReadNextNumberFromType2CharString(EnhancedBinaryReader reader, out uint outNumber, out bool isOperator)
        {
            outNumber = 0;
            isOperator = false;

            return false;
        }

        private bool SelectLocalSubr(out CFFIndexTable indexToUse, ushort glyphIndex)
        {
            indexToUse = null;

            if(_fdSelect.Count > 0 && _localSubrsPerFont.Count == 0)
            {
                byte fdIndex = 0;
                if (!_fdSelect.TryGetValue(glyphIndex, out fdIndex))
                    return false; // TODO failure -- glyph Index not found

                if (fdIndex >= _localSubrsPerFont.Count)
                    return false; // TODO failure - fdIndex exceeds local subrs per font list size.

                indexToUse = _localSubrsPerFont[fdIndex];
            }else if(_localSubrs != null)
            {
                // Second, try to use |local_subrs|. Most Latin fonts don't have FDSelect
                // entries. If The font has a local subrs index associated with the Top
                // DICT (not FDArrays), use it.
                indexToUse = _localSubrs;
            }
            else
            {
                // Just return null
                indexToUse = null;
            }

            return true;
        }

        private void ParseNameData(EnhancedBinaryReader reader, CFFIndexTable index)
        {
            for(int i = 0; i < index.Objects.Length; i++)
            {
                SetLocalOffset(reader, index.Objects[i].Offset);
                FontName = reader.ReadString((int)index.Objects[i].DataSize);
            }
        }

        private void ParseDictData(EnhancedBinaryReader reader, Logger log, CFFIndexTable index, DictDataType dataType, ushort numGlyphs, uint sidMax)
        {
            for (int i = 0; i < index.Length; i++)
            {
                SetLocalOffset(reader, index.Objects[i].Offset);

                List<KeyValuePair<uint, DictOperandType>> operands = new List<KeyValuePair<uint, DictOperandType>>();
                FontFormat fontFormat = FontFormat.Uknown;
                bool haveRos = false;
                int charStringGlyphs = 0;
                uint charSetOffset = 0;
                long curStreamPos = 0;
                byte format = 0;

                long endPos = GetLocalOffset(reader) + index.Objects[i].DataSize;
                while (GetLocalOffset(reader) < endPos)
                {
                    if (!ParseDictDataReadNext(reader, operands))
                        return; // TODO failure

                    if (operands[operands.Count - 1].Value != DictOperandType.Operator)
                        continue;

                    // Got operator
                    uint op = operands[operands.Count - 1].Key;
                    operands.RemoveAt(operands.Count - 1);

                    switch (op)
                    {
                        // SID
                        case 0: // version
                        case 1: // Notice
                        case 2: // Copyright
                        case 3: // Full name
                        case 4: // Family name
                        case (12U << 8) + 0: // Copyright
                        case (12U << 8) + 21: // PostScript
                        case (12U << 8) + 22: // Base font name
                        case (12U << 8) + 38: // Font name
                            if (operands.Count != 1)
                                return; // TODO failure

                            if (!CheckSid(operands[operands.Count - 1], sidMax))
                                return; // TODO failure
                            break;

                        // Array
                        case 5: // Font bounding box
                        case 14: // XUID
                        case (12U << 8) + 7: // Font matrix
                        case (12U << 8) + 23: // Base font blend (delta)
                            if (operands.Count == 0)
                                return; // TODO failure
                            break;

                        // Number
                        case 13:  // UniqueID
                        case (12U << 8) + 2:   // ItalicAngle
                        case (12U << 8) + 3:   // UnderlinePosition
                        case (12U << 8) + 4:   // UnderlineThickness
                        case (12U << 8) + 5:   // PaintType
                        case (12U << 8) + 8:   // StrokeWidth
                        case (12U << 8) + 20: // SyntheticBase
                            if (operands.Count != 1)
                                return; // TODO failure;
                            break;

                        case (12U << 8) + 31:  // CIDFontVersion
                        case (12U << 8) + 32:  // CIDFontRevision
                        case (12U << 8) + 33:  // CIDFontType
                        case (12U << 8) + 34:  // CIDCount
                        case (12U << 8) + 35: // UIDBase
                            if (operands.Count != 1)
                                return; // TODO failure

                            if (fontFormat != FontFormat.CID_Keyed)
                                return; // TODO failure
                            break;
                        case (12U << 8) + 6: // CharstringType
                            if (operands.Count != 1)
                                return; // TODO failure

                            if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                                return; // TODO failure

                            if (operands[operands.Count - 1].Key != 2) // We only support the "Type 2 Charstring Format." -- TODO: Support Type 1 format? Is that still in use?
                                return; // TODO failure.
                            break;

                        // boolean
                        case (12U << 8) + 1: // isFixedPitch  
                            if (operands.Count != 1)
                                return; // TODO failure
                            if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                                return; // TODO failure                            
                            if (operands[operands.Count - 1].Key >= 2)
                                return; // TODO failure.
                            break;

                        // offset(0)
                        case 15: // charset
                            if (operands.Count != 1)
                                return; // TODO failure
                            if (operands[operands.Count - 1].Key <= 2)
                                break; // Predefined charset, ISOAdobe, Expert or ExpertSubset, is used.
                            // TODO CheckOffset()
                            if (charSetOffset > 0)
                                return; // TODO failure
                            charSetOffset = operands[operands.Count - 1].Key;
                            break;

                        // Encoding
                        case 16:
                            if (operands.Count != 1)
                                return; // TODO failure
                            if (operands[operands.Count - 1].Key <= 1)
                                break; // predefined encoding, "Standard" or "Expert", is used.

                            // TODO CheckOffset()

                            // Parse sub-dictionary INDEX
                            SetLocalOffset(reader, operands[operands.Count - 1].Key);
                            format = reader.ReadByte();
                            if ((format & 0x80) == 0x80)
                                return; // TODO failure -- Supplemental encoding is not supported at the moment.

                            // TODO Parse supplemental encoding tables.
                            break;

                        // CharStrings
                        case 17:
                            if (dataType != DictDataType.TopLevel)
                                return; // TODO failure
                            if (operands.Count != 1)
                                return; // TODO failure
                            // TODO CheckOffset()

                            // Parse Charstrings INDEX
                            curStreamPos = reader.Position;
                            uint charStringsIndexOffset = operands[operands.Count - 1].Key;
                            CFFIndexTable charStringIndex = new CFFIndexTable(reader, log, this, charStringsIndexOffset);
                            reader.Position = curStreamPos;

                            if (charStringIndex.Length < 2)
                                return; // TODO failure

                            if (charStringGlyphs > 0)
                                return; // TODO failure - Multiple charstring tables?

                            charStringGlyphs = charStringIndex.Length;
                            if (charStringGlyphs != numGlyphs)
                                return; // TODO failure - CFF and maxp have different number of glyphs?
                            break;

                        // FDArray
                        case (12U << 8) + 36:
                            if (dataType != DictDataType.TopLevel)
                                return; // TODO failure
                            if (operands.Count != 1)
                                return; // TODO failure
                                        // TODO CheckOffset()

                            // Parse sub-dictionary INDEX
                            curStreamPos = reader.Position;
                            uint fdArrayIndexOffset = operands[operands.Count - 1].Key;
                            CFFIndexTable subDictIndex = new CFFIndexTable(reader, log, this, fdArrayIndexOffset);
                            ParseDictData(reader, log, subDictIndex, DictDataType.FdArray, numGlyphs, sidMax);
                            reader.Position = curStreamPos;
                            if (_fontDictLength > 0)
                                return; // TODO failure - two or more FDArray found.

                            _fontDictLength = subDictIndex.Length;
                            break;

                        //FDSelect
                        case (12U << 8) + 37:
                            if (dataType != DictDataType.TopLevel)
                                return; // TODO failure
                            if (operands.Count != 1)
                                return; // TODO failure
                                        // TODO CheckOffset()

                            // Parse FDSelect data structure
                            curStreamPos = reader.Position;
                            SetLocalOffset(reader, operands[operands.Count - 1].Key);
                            format = reader.ReadByte();
                            if (format == 0)
                            {
                                for (ushort j = 0; j < numGlyphs; j++)
                                {
                                    byte fdIndex = reader.ReadByte();
                                    _fdSelect[j] = fdIndex;
                                }
                            }
                            else if (format == 3)
                            {
                                ushort nRanges = reader.ReadUInt16();
                                ushort lastGid = 0;
                                byte fdIndex = 0;
                                for (ushort j = 0; j < nRanges; j++)
                                {
                                    ushort first = reader.ReadUInt16(); // GID
                                    // Sanity checks
                                    if (j == 0 && first != 0)
                                        return; // TODO failure

                                    if (j != 0 && lastGid >= first)
                                        return; // TODO failure - not increasing order.

                                    // Copy the mapping to _fdSelect
                                    if (j != 0)
                                    {
                                        for (ushort k = lastGid; k < first; k++)
                                        {
                                            _fdSelect.Add(k, fdIndex);
                                            if (fdIndex == 0)
                                                return; // TODO failure
                                        }
                                    }

                                    fdIndex = reader.ReadByte();
                                    lastGid = first;
                                    // TODO Check GID?
                                }

                                ushort sentinel = reader.ReadUInt16();
                                if (lastGid >= sentinel)
                                    return; // TODO Failure

                                for(ushort k = lastGid; k < sentinel; k++)
                                {
                                    _fdSelect.Add(k, fdIndex);
                                    if (fdIndex == 0) // TODO do we use a nullable here? Is 0 a valid number?
                                        return; // TODO failure
                                }
                            }
                            else
                            {
                                return; // TODO failure - unknown format
                            }
                            break;

                        // Private DICT (2 * number)
                        case 18:
                            if (operands.Count != 2)
                                return; // TODO failure
                            if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                                return; // TODO failure

                            uint privateOffset = operands[operands.Count - 1].Key;
                            operands.RemoveAt(operands.Count - 1);
                            if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                                return; // TODO failure

                            uint privateLength = operands[operands.Count - 1].Key;

                            // Parse pirvate DICT data
                            if (!ParsePrivateDictData(reader, log, privateOffset, privateLength, dataType))
                                return; // TODO failure - invalid private DICT data.
                            break;

                        // ROS
                        case (12U << 8) + 30:
                            if (fontFormat != FontFormat.Uknown)
                                return; // TODO failure
                            fontFormat = FontFormat.CID_Keyed;
                            if (operands.Count != 3)
                                return; // TODO failure - incorrect operands in ROS data

                            // Check SIDs
                            operands.RemoveAt(operands.Count - 1); // Ignore first number
                            if (!CheckSid(operands[operands.Count - 1], sidMax))
                                return; // TODO failure

                            operands.RemoveAt(operands.Count - 1);
                            if (!CheckSid(operands[operands.Count - 1], sidMax))
                                return; // TODO failure

                            if (haveRos)
                                return; // TODO failure - Multiple ROS tables?
                            haveRos = true;
                            break;
                    }

                    operands.Clear();
                    if (fontFormat == FontFormat.Uknown)
                        fontFormat = FontFormat.Other;
                } // While loop end

                // Parse char sets
                if(charSetOffset > FontUtil.NULL)
                {
                    curStreamPos = reader.Position;
                    SetLocalOffset(reader, charSetOffset);
                    format = reader.ReadByte();
                    switch (format)
                    {
                        case 0:
                            for(ushort j = 1  /* .notdef is omitted */; j < numGlyphs; j++)
                            {
                                ushort sid = reader.ReadUInt16();
                                if (!haveRos && (sid > sidMax))
                                    return; // TODO failure

                                // TODO Check CIDs when haveRos is true.
                            }
                            break;

                        case 1:
                        case 2:
                            uint total = 1; // .notdef is omitted.
                            while(total < numGlyphs)
                            {
                                ushort sid = reader.ReadUInt16();
                                if (!haveRos && sid > sidMax)
                                    return; // TODO failure

                                // TODO Check CIDs when haveRos is true.

                                if (format == 1) // TODO clean this IF block up
                                {
                                    byte left = reader.ReadByte();
                                    total += (left + 1U);
                                }
                                else
                                {
                                    ushort left = reader.ReadUInt16();
                                    total += (left + 1U);
                                }
                            }
                            break;
                        default:
                            return; // TODO failure - unknown/invalid CharSet format
                    }
                }
            }
        }

        private bool ParsePrivateDictData(EnhancedBinaryReader reader, Logger log, uint offset, uint privateLength, DictDataType dataType)
        {
            uint dataStartPos = GetLocalOffset(reader);
            uint startPos = 0;

            SetLocalOffset(reader, offset);
            List<KeyValuePair<uint, DictOperandType>> operands = new List<KeyValuePair<uint, DictOperandType>>();
            long endPos = GetLocalOffset(reader) + privateLength;
            while(GetLocalOffset(reader) < endPos)
            {
                if (!ParseDictDataReadNext(reader, operands))
                    return false; // TODO failure

                if (operands.Count == 0)
                    return false; // TODO failure

                if (operands.Count > 48) // An operator may be preceded by up to a maximum of 48 operands.
                    return false; // TODO failure

                if (operands[operands.Count - 1].Value != DictOperandType.Operator)
                    continue;

                // Got operator
                uint op = operands[operands.Count - 1].Key;
                operands.RemoveAt(operands.Count - 1);
                switch (op)
                {
                    // Hints
                    case 6:  // BlueValues
                    case 7:  // OtherBlues
                    case 8:  // FamilyBlues
                    case 9: // FamilyOtherBlues
                        if (operands.Count % 2 != 0)
                            return false; // TODO failure
                        break;

                    // Array
                    case (12U << 8) + 12:  // StemSnapH (delta)
                    case (12U << 8) + 13: // StemSnapV (delta)
                        if (operands.Count == 0)
                            return false; // TODO failure
                        break;

                    // Number
                    case 10:  // StdHW
                    case 11:  // StdVW
                    case 20:  // defaultWidthX
                    case 21:  // nominalWidthX
                    case (12U << 8) + 9:   // BlueScale
                    case (12U << 8) + 10:  // BlueShift
                    case (12U << 8) + 11:  // BlueFuzz
                    case (12U << 8) + 17:  // LanguageGroup
                    case (12U << 8) + 18:  // ExpansionFactor
                    case (12U << 8) + 19: // initialRandomSeed
                        if (operands.Count != 1)
                            return false; // TODO failure
                        break;

                    // Local Subrs INDEX, offset(self)
                    case 19:
                        if (operands.Count != 1)
                            return false; // TODO failure

                        if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                            return false; // TODO failure

                        if (operands[operands.Count - 1].Key >= (1024 * 1024 * 1024)) // TODO cache product of multiplication in constant
                            return false; // TODO failure

                        if (operands[operands.Count - 1].Key + offset >= reader.BaseStream.Length)
                            return false; // TODO failure - offset out of bounds of table.

                        // Parse local subrs INDEX
                        startPos = GetLocalOffset(reader);
                        CFFIndexTable localSubrsIndex = new CFFIndexTable(reader, log, this, operands[operands.Count - 1].Key + offset);
                        SetLocalOffset(reader, startPos);
                        _localSubrsPerFont.Add(localSubrsIndex);
                        if (dataType == DictDataType.FdArray)
                        {
                            // Nothing to check. LocalSubRsPerFont is never empty at this point.
                        }
                        else // dataType == DictOperandType.TopLevel
                        {
                            if (_localSubrs != null)
                                return false; // TODO failure -- Two or more LocalSubrs?

                            _localSubrs = localSubrsIndex;
                        }
                        break;

                    // boolean
                    case (12U << 8) + 14: // ForceBold
                        if (operands.Count != 1)
                            return false; // TODO failure
                        if (operands[operands.Count - 1].Value != DictOperandType.Integer)
                            return false; // TODO failure
                        if (operands[operands.Count - 1].Key >= 2)
                            return false; // TODO failure
                        break;

                    default:
                        return false; // TODO failure
                }
                operands.Clear();
            }

            SetLocalOffset(reader, dataStartPos);
            return true;
        }

        private bool CheckSid(KeyValuePair<uint, DictOperandType> operand, uint sidMax)
        {
            if (operand.Value != DictOperandType.Integer)
                return false;

            if (operand.Key > sidMax)
                return false;

            return true;
        }

        private bool ParseDictDataReadNext(EnhancedBinaryReader reader, List<KeyValuePair<uint, DictOperandType>> operands)
        {
            uint op = reader.ReadByte();

            if (op <= 22)
            {
                // Check if escaped operator.
                if (op == 12)
                {
                    op = reader.ReadByte();
                    if ((op <= 14) || (op >= 17 && op <= 23) || (op >= 30 && op <= 38))
                        operands.Add(new KeyValuePair<uint, DictOperandType>((12U << 8) + op, DictOperandType.Operator));
                }
                else
                {
                    operands.Add(new KeyValuePair<uint, DictOperandType>(op, DictOperandType.Operator));
                }
            }
            else if (op <= 27 || op == 31 || op == 255)
            {
                // Reserved 
                return false; // TODO failure -- Invalid operator?
            }

            ParseDictDataNumber(reader, op, operands);
            return true;
        }

        private void ParseDictDataNumber(EnhancedBinaryReader reader, uint b0, List<KeyValuePair<uint, DictOperandType>> operands)
        {
            uint b1 = 0, b2 = 0, b3 = 0, b4 = 0;

            switch (b0)
            {
                case 28: // shortint
                    b1 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    operands.Add(new KeyValuePair<uint, DictOperandType>((b1 << 8) + b2, DictOperandType.Integer)); // TODO replace with .ReadUInt16() when confirmed to work.
                    return;

                case 29: // longint
                    b1 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    b3 = reader.ReadByte();
                    b4 = reader.ReadByte();
                    operands.Add(new KeyValuePair<uint, DictOperandType>((b1 << 24) + (b2 << 16) + (b3 << 8) + b4, DictOperandType.Integer));// TODO replace with .ReadUInt32() when confirmed to work.
                    return;

                case 30: // binary coded decimal (BCD)
                    ParseDictDataBcd(reader, operands);
                    return;
            }

            uint result = 0;
            if(b0 >= 32 && b0 <= 246)
            {
                result = b0 - 139;
            }else if(b0 >= 247 && b0 <= 250)
            {
                b1 = reader.ReadByte();
                result = (b0 - 247) * 256 + b1 + 108;
            }else if(b0 >= 251 && b0 <= 254)
            {
                b1 = reader.ReadByte();
                result = (uint)(-(b0 - 251U) * 256U + b1 - 108U);
            }
            else
            {
                // Something went wrong. Failed.
                return; // TODO failure
            }

            operands.Add(new KeyValuePair<uint, DictOperandType>(result, DictOperandType.Integer));
        }

        private void ParseDictDataBcd(EnhancedBinaryReader reader, List<KeyValuePair<uint, DictOperandType>> operands)
        {
            bool readDecimalPoint = false;
            bool readE = false; // exponential

            // TODO try replacing all this with reader.ReadFloat or ReadDouble. 
            byte nibble = 0;
            uint count = 0;
            while (true)
            {
                nibble = reader.ReadByte();
                if((nibble & 0xf0) == 0xf0)
                {
                    if((nibble & 0xf) == 0xf)
                    {
                        // TODO: would be better to store actual double value, rather than the dummy integer.
                        operands.Add(new KeyValuePair<uint, DictOperandType>(0, DictOperandType.Real));
                        return;
                    }
                }

                if((nibble & 0x0f) == 0x0f)
                {
                    operands.Add(new KeyValuePair<uint, DictOperandType>(0, DictOperandType.Real));
                    return;
                }

                // Check number format
                uint[] nibbles = new uint[2];
                nibbles[0] = (nibble & 0xf0U) >> 8;
                nibbles[1] = (nibble & 0x0fU);
                for(int i = 0; i < 2; i++)
                {
                    if(nibbles[i] == 0xD) // Reserved number
                        return;// TODO failure

                    if ((nibbles[i] == 0xE) && (count > 0 || i > 0))
                        return; // TODO failure - minus sign should be the first character.

                    if(nibbles[i] == 0xA) // Decimal point
                    {
                        if (!readDecimalPoint)
                            readDecimalPoint = true;
                        else
                            return; // TODO failure - two or more decimal points.
                    }

                    if(nibbles[i] == 0xB || nibbles[i] == 0XC) // E+ or E- (same order in IF condition)
                    {
                        if (!readE)
                            readE = true;
                        else
                            return; // TODO failure - two or more E's (exponential).
                    }
                }

                count++;
            }
        }

        private enum DictDataType
        {
            TopLevel = 0,

            FdArray = 1,
        }

        private enum DictOperandType
        {
            Integer = 0,

            Real = 1,

            Operator = 2,
        }

        private enum FontFormat
        {
            Uknown = 0,

            CID_Keyed = 1,

            Other = 2, // Including synthetic fonts.
        }
    }
}
