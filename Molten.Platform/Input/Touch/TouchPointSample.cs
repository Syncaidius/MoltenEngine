using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct TouchPointSample
    {
        public Vector2F Position;

        public Vector2F Delta;

        public TouchPointState State;

        public int ID;
    }
}
