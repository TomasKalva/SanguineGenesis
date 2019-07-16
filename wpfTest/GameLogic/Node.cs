using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class Node
    {
        public int X { get; }
        public int Y { get; }
        public Terrain Terrain { get; set; }
        public bool Blocked { get; private set; }

        public Node(int x, int y, Terrain t)
        {
            this.Terrain = t;
            X = x;
            Y = y;
            Blocked = false;
        }
    }
}
