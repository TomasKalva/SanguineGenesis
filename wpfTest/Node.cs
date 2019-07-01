using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class Node
    {
        public Terrain Terrain { get; set; }
        public int X { get; }
        public int Y { get; }

        public Node(Terrain t,int x, int y)
        {
            this.Terrain = t;
            X = x;
            Y = y;
        }
    }
}
