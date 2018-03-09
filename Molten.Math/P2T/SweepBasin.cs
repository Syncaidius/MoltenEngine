using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SweepBasin
    {
        public Node left_node;

        public Node bottom_node;

        public Node right_node;

        public double width;

        public bool left_highest;

        public void Clear()
        {
            left_node = null;
            bottom_node = null;
            right_node = null;
            width = 0;
            left_highest = false;
        }
    }
}
