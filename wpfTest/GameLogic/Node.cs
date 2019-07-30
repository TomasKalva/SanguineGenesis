using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class Node:ITargetable
    {
        public int X { get; }
        public int Y { get; }
        public Terrain Terrain { get; set; }
        public bool Blocked { get; private set; }
        Vector2 ITargetable.Center => new Vector2(X + 0.5f, Y + 0.5f);

        public Node(int x, int y, Terrain t)
        {
            this.Terrain = t;
            X = x;
            Y = y;
            Blocked = false;
        }
    }
}
