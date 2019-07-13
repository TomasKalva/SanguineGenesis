using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public struct UnitView
    {
        public Vector2 Pos { get; }
        public float Range { get; }

        public UnitView(Vector2 pos, float range)
        {
            Pos = pos;
            Range = range;
        }
    }
}
