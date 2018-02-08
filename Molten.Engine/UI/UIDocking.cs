using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    [FlagsAttribute]
    public enum UIDocking : byte
    {
        None = 0,

        Left = 1,

        Right = 2,

        Top = 4,

        Bottom = 8,

        Horizonal = 16,

        Vertical = 32,

        //combinations
        /// <summary>Horizontal + vertical.</summary>
        Center = 48,

        /// <summary>Vertical + Left.</summary>
        CenterLeft = 33,

        /// <summary>Vertical + Right.</summary>
        CenterRight = 34,

        /// <summary>Top + Horizontal.</summary>
        CenterTop = 20,

        /// <summary>Bottom + Horizontal.</summary>
        CenterBottom = 24,

        TopLeft = 5,

        TopRight = 6,

        BottomLeft = 9,

        BottomRight = 10,
    }
}
