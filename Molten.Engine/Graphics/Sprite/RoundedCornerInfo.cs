using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct RoundedCornerInfo
    {
        public float TopLeftRadius;

        public float TopRightRadius;

        public float BottomRightRadius;

        public float BottomLeftRadius;

        public RoundedCornerInfo(float tl, float tr, float br, float bl)
        {
            TopLeftRadius = tl;
            TopRightRadius = tr;
            BottomRightRadius = br;
            BottomLeftRadius = bl;
        }

        /// <summary>
        /// Returns true if at least one corner has a radius greater than 0.
        /// </summary>
        /// <returns></returns>
        public bool HasRounded()
        {
            return TopLeftRadius > 0 || TopRightRadius > 0 || BottomRightRadius > 0 || BottomLeftRadius > 0;
        }

        /// <summary>
        /// Returns true if all corners have the same radius.
        /// </summary>
        /// <returns></returns>
        public bool OneRadius()
        {
            return TopLeftRadius == TopRightRadius && TopRightRadius == BottomRightRadius && BottomRightRadius == BottomLeftRadius;
        }

        /// <summary>
        /// Returns true if the left edge has the same radius for top-left and bottom-left corners.
        /// </summary>
        /// <returns></returns>
        public bool LeftOneRadius()
        {
            return TopLeftRadius == BottomLeftRadius;
        }

        /// <summary>
        /// Returns true if the right edge has the same radius for top-right and bottom-right corners.
        /// </summary>
        /// <returns></returns>
        public bool RightOneRadius()
        {
            return TopRightRadius == BottomRightRadius;
        }

        /// <summary>
        /// Returns true if the bottom edge has the same radius for bottom-left and bottom-right corners.
        /// </summary>
        /// <returns></returns>
        public bool BottomOneRadius()
        {
            return BottomLeftRadius == BottomRightRadius;
        }

        /// <summary>
        /// Returns true if the top edge has the same radius for top-left and top-right corners.
        /// </summary>
        /// <returns></returns>
        public bool TopOneRadius()
        {
            return TopLeftRadius == TopRightRadius;
        }
    }
}
