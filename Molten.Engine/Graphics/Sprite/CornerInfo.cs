using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct CornerInfo
    {
        public float TopLeft;

        public float TopRight;

        public float BottomRight;

        public float BottomLeft;

        public CornerInfo(float tl, float tr, float br, float bl)
        {
            TopLeft = tl;
            TopRight = tr;
            BottomRight = br;
            BottomLeft = bl;
        }

        public CornerInfo(float radius)
        {
            TopLeft = radius;
            TopRight = radius;
            BottomLeft = radius;
            BottomRight = radius;
        }

        public CornerInfo Restrict(float max)
        {
            return new CornerInfo()
            {
                TopLeft = TopLeft > max ? max : TopLeft,
                TopRight = TopRight > max ? max : TopRight,
                BottomLeft = BottomLeft > max ? max : BottomLeft,
                BottomRight = BottomRight > max ? max : BottomRight,
            };
        }

        /// <summary>
        /// Returns true if at least one corner has a radius greater than 0.
        /// </summary>
        /// <returns></returns>
        public bool HasRounded()
        {
            return TopLeft > 0 || TopRight > 0 || BottomRight > 0 || BottomLeft > 0;
        }

        /// <summary>
        /// Returns true if all corners have the same radius.
        /// </summary>
        /// <returns></returns>
        public bool OneRadius()
        {
            return TopLeft == TopRight && TopRight == BottomRight && BottomRight == BottomLeft;
        }

        /// <summary>
        /// Returns true if the left edge has the same radius for top-left and bottom-left corners.
        /// </summary>
        /// <returns></returns>
        public bool LeftSameRadius()
        {
            return TopLeft == BottomLeft;
        }

        /// <summary>
        /// Returns true if the right edge has the same radius for top-right and bottom-right corners.
        /// </summary>
        /// <returns></returns>
        public bool RightSameRadius()
        {
            return TopRight == BottomRight;
        }

        /// <summary>
        /// Returns true if the bottom edge has the same radius for bottom-left and bottom-right corners.
        /// </summary>
        /// <returns></returns>
        public bool BottomSameRadius()
        {
            return BottomLeft == BottomRight;
        }

        /// <summary>
        /// Returns true if the top edge has the same radius for top-left and top-right corners.
        /// </summary>
        /// <returns></returns>
        public bool TopSameRadius()
        {
            return TopLeft == TopRight;
        }

        public bool LeftHasRadius()
        {
            return TopLeft > 0 || BottomLeft > 0;
        }

        public bool RightHasRadius()
        {
            return TopRight > 0 || BottomRight > 0;
        }
    }
}
