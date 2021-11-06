using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct Touch3PointGuesture : ITouchGesture
    {
        public Vector2F Point1;

        public Vector2F Point2;

        /// <summary>
        /// The amount of change between <see cref="Point1"/> and <see cref="Point2"/> 
        /// positions since this gesture was last updated.
        /// </summary>
        public Vector2F Delta;

        public TouchPointState State;

        public int ID { get; }
    }
}
