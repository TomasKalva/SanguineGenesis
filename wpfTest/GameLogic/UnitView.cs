using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public struct View
    {
        public Vector2 Pos { get; }
        public float Range { get; }

        public View(Vector2 pos, float range)
        {
            Pos = pos;
            Range = range;
        }
    }
}
