namespace Molten.Graphics.Textures.DDS
{
    public class DDSReader : TextureReader
    {
        TextureSlice[] _levelData;
        string _magicWord;
        DDSHeader _header;
        DDSHeaderDXT10 _headerDXT10;

        bool _isCubeMap;
        bool[] _cubeSides;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="reader"></param>
        /// <param name="decompress">If set to true, reader will decompress textures that are compressed.</param>
        public DDSReader()
        {
            _isCubeMap = false;
            _cubeSides = new bool[6];
        }

        public override TextureData Read(BinaryReader reader, Logger log, string filename = null)
        {
            if (!ReadHeader(reader, log, filename) || !ReadData(reader, log, filename))
                return null;

            TextureData data = new TextureData(_headerDXT10.ArraySize, _levelData)
            {
                Levels = _levelData,
                Format = _headerDXT10.ImageFormat,
                Flags = GraphicsResourceFlags.GpuRead | GraphicsResourceFlags.GpuWrite,
                IsCompressed = true,
                MultiSampleLevel = AntiAliasLevel.None,
                HighestMipMap = 0,
            };

            return data;
        }

        private bool ReadHeader(BinaryReader reader, Logger log, string filename = null)
        {
            // Make sure the stream is at least big enough to contain a complete DDS header.
            if (reader.BaseStream.Length < 128)
            {
                log.Error("DDS header is invalid.", filename);
                return false;
            }

            _magicWord = GetMagicWord(reader.ReadUInt32());
            _magicWord = _magicWord.Trim();
            if (_magicWord != "DDS")
            {
                log.Error("Data does not contain valid DDS data. Magic word not found.", filename);
                return false;
            }

            _header = new DDSHeader()
            {
                Size = reader.ReadUInt32(),
                Flags = (DDSFlags)reader.ReadUInt32(),
                Height = reader.ReadUInt32(),
                Width = reader.ReadUInt32(),
                PitchOrLinearSize = reader.ReadUInt32(),
                Depth = reader.ReadUInt32(),
                MipMapCount = reader.ReadUInt32(),
                Reserved = GetReserved(reader, 11),
                PixelFormat = new DDSPixelFormat()
                {
                    Size = reader.ReadUInt32(),
                    Flags = (DDSPixelFormatFlags)reader.ReadUInt32(),
                    FourCC = GetFourCC(reader.ReadUInt32()),
                    RGBBitCount = reader.ReadUInt32(),
                    RBitMask = reader.ReadUInt32(),
                    GBitMask = reader.ReadUInt32(),
                    BBitMask = reader.ReadUInt32(),
                    ABitMask = reader.ReadUInt32(),
                },

                Caps = (DDSCapabilities)reader.ReadUInt32(),
                Caps2 = (DDSAdditionalCapabilities)reader.ReadUInt32(),
                Caps3 = reader.ReadUInt32(),
                Caps4 = reader.ReadUInt32(),
                Reserved2 = reader.ReadUInt32(),
            };

            //set default DX10 header info
            _headerDXT10.ArraySize = 1;

            //figure out which block parser to use
            if (_header.PixelFormat.HasFlags(DDSPixelFormatFlags.FourCC))
            {
                switch (_header.PixelFormat.FourCC)
                {
                    case "DXT1":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC1_UNorm;
                        break;

                    case "DXT2":
                    case "DXT3":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC2_UNorm;
                        break;

                    case "DXT4":
                    case "DXT5":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC3_UNorm;
                        break;

                    case "BC4U":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC4_UNorm;
                        break;

                    case "BC4S":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC4_SNorm;
                        break;

                    case "BC5U":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC5_UNorm;
                        break;

                    case "BC5S":
                        _headerDXT10.ImageFormat = GraphicsFormat.BC5_SNorm;
                        break;

                    case "DX10":
                        //read DX10 header.
                        _headerDXT10 = new DDSHeaderDXT10()
                        {
                            ImageFormat = (GraphicsFormat)reader.ReadUInt32(),
                            Dimension = (DDSResourceDimension)reader.ReadUInt32(),
                            MiscFlags = (DDSMiscFlags)reader.ReadUInt32(),
                            ArraySize = reader.ReadUInt32(),
                            MiscFlags2 = (DDSMiscFlags2)reader.ReadUInt32(),
                        };
                        break;

                    default:
                        log.Error($"Unrecognised DDS block-compression format '{_header.PixelFormat.FourCC}'", filename);
                        _headerDXT10.ImageFormat = GraphicsFormat.R8G8B8A8_UNorm;
                        break;
                }
            }

            CheckIfCubeMap();

            // Ensure there is at least one level mip texture (the main level).
            _header.MipMapCount = Math.Max(1, _header.MipMapCount);

            return true;
        }

        private void CheckIfCubeMap()
        {
            //Order , x+, x-, y+, y-, z+, z-

            if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMap) == DDSAdditionalCapabilities.CubeMap)
            {
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapPositiveX) == DDSAdditionalCapabilities.CubeMapPositiveX)
                    _cubeSides[0] = true;
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapNegativeX) == DDSAdditionalCapabilities.CubeMapNegativeX)
                    _cubeSides[1] = true;
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapPositiveY) == DDSAdditionalCapabilities.CubeMapPositiveY)
                    _cubeSides[2] = true;
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapNegativeY) == DDSAdditionalCapabilities.CubeMapNegativeY)
                    _cubeSides[3] = true;
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapPositiveZ) == DDSAdditionalCapabilities.CubeMapPositiveZ)
                    _cubeSides[4] = true;
                if ((_header.Caps2 & DDSAdditionalCapabilities.CubeMapNegativeZ) == DDSAdditionalCapabilities.CubeMapNegativeZ)
                    _cubeSides[5] = true;

                //check if the DDS is a cube map. This is true if it contains even 1 cube map side.
                for (int i = 0; i < 6; i++)
                {
                    if (_cubeSides[i])
                    {
                        _isCubeMap = true;

                        //manually set DX10 header info.
                        _headerDXT10.ArraySize = 6;
                        _headerDXT10.Dimension = DDSResourceDimension.Texture3D;
                        _headerDXT10.MiscFlags = DDSMiscFlags.TextureCube;
                        break;
                    }
                }
            }

            //if the DDS wasn't detected as a legacy cube-map, try using the DX10 approach.
            if (_isCubeMap == false)
            {
                if (_headerDXT10.Dimension == DDSResourceDimension.Texture3D)
                {
                    if (_headerDXT10.ArraySize == 6)
                    {
                        if (_headerDXT10.MiscFlags == DDSMiscFlags.TextureCube)
                        {
                            //set all sides to present (true).
                            _isCubeMap = true;
                            for (int i = 0; i < 6; i++)
                                _cubeSides[i] = true;
                        }
                    }
                }
            }
        }

        private unsafe bool ReadData(BinaryReader reader, Logger log, string filename = null)
        {
            // Check for invalid mip map values.
            if (_header.MipMapCount > 512)
            {
                log.Error($"Invalid mip-map count: {_header.MipMapCount}", filename);
                return false;
            }

            _levelData = new TextureSlice[_header.MipMapCount * _headerDXT10.ArraySize];
            uint blockSize = BCHelper.GetBlockSize(_headerDXT10.ImageFormat);

            for (uint a = 0; a < _headerDXT10.ArraySize; a++)
            {
                uint levelWidth = _header.Width;
                uint levelHeight = _header.Height;

                for (uint i = 0; i < _header.MipMapCount; i++)
                {
                    uint blockPitch, levelByteSize;
                    BCHelper.GetBCLevelSizeAndPitch(levelWidth, levelHeight, blockSize, out levelByteSize, out blockPitch);

                    byte[] data = reader.ReadBytes((int)levelByteSize);
                    if (data.Length != levelByteSize)
                    {
                        log.Warning($"Unexpeceted BC level size -- Expected: {levelByteSize} -- Actual: {data.Length}");
                        levelByteSize = (uint)data.Length;
                    }

                    TextureSlice level = new TextureSlice(levelWidth, levelHeight, data)
                    {
                        Pitch = blockPitch,
                    };

                    uint dataID = (a * _header.MipMapCount) + i;
                    _levelData[dataID] = level;

                    // Decrease level width/height ready for next read
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            return true;
        }


        /// <summary>Calculates the expected mip-map count.</summary>
        /// <returns></returns>
        private uint GetMipMapCount()
        {
            uint curSize = _header.Width;
            uint levels = 1;

            while (curSize > 1)
            {
                curSize /= 2;
                levels++;
            }

            return levels;
        }

        private uint[] GetReserved(BinaryReader reader, int count)
        {
            uint[] reserved = new uint[count];

            for (int i = 0; i < count; i++)
                reserved[i] = reader.ReadUInt32();

            return reserved;
        }

        private string GetMagicWord(uint value)
        {
            string magic = "";

            magic += (char)((value & 0xff)); // First character
            magic += (char)((value & 0xff00) >> 8); // Second character
            magic += (char)((value & 0xff0000) >> 16); // Third character
            magic += (char)((value & 0xff000000) >> 24); // Fourth character
            return magic;
        }

        private string GetFourCC(uint value)
        {
            string magic = "";

            // Read 4 characters, in reverse. This results in them being ordered correctly.
            for (int i = 3; i >= 0; i--)
            {
                uint shifted = value << (8 * i); //shift forward to remove unneeded chars
                shifted = shifted >> 24; //shift back 3 bytes (24 bit = 8 bit * 3)
                char c = (char)shifted;
                magic += c;
            }

            return magic;
        }
    }
}
