using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class AnchorTable : FontSubTable
    {
        /// <summary>
        /// Gets the format of the anchor table.
        /// </summary>
        public ushort Format { get; internal set; }

        /// <summary>
        /// Gets the horizontal value, in design units
        /// </summary>
        public short XCoordinate { get; internal set; }

        /// <summary>
        /// Gets the vertical value, in design units
        /// </summary>
        public short YCoordinate { get; internal set; }

        /// <summary>
        /// Gets an index to glyph contour point. Only set if <see cref="Format"/> is 2.
        /// </summary>
        public ushort AnchorPoint { get; internal set; }

        /// <summary>
        /// Gets a Device table (non-variable font) / VariationIndex table (variable font) for X coordinate, from beginning of Anchor table (may be null)
        /// </summary>
        public DeviceVariationIndexTable XDevice { get; internal set; }

        /// <summary>
        /// Gets a Device table (non-variable font) / VariationIndex table (variable font) for Y coordinate, from beginning of Anchor table (may be null)
        /// </summary>
        public DeviceVariationIndexTable YDevice { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            Format = reader.ReadUInt16();

            switch (Format)
            {
                case 1:
                    XCoordinate = reader.ReadInt16();
                    YCoordinate = reader.ReadInt16();
                    break;

                case 2:
                    XCoordinate = reader.ReadInt16();
                    YCoordinate = reader.ReadInt16();
                    AnchorPoint = reader.ReadUInt16();
                    break;

                case 3:
                    ushort xDeviceOffset = reader.ReadUInt16();
                    ushort yDeviceOffset = reader.ReadUInt16();

                    if (xDeviceOffset > FontUtil.NULL)
                        XDevice = context.ReadSubTable<DeviceVariationIndexTable>(xDeviceOffset);

                    if (yDeviceOffset > FontUtil.NULL)
                        YDevice = context.ReadSubTable<DeviceVariationIndexTable>(yDeviceOffset);
                    break;
            }
        }
    }
}
