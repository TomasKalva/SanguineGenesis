using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class SelectorFrameInput
    {
        public bool Selecting { get; private set; }
        public Vector2 MapCoordinates { get; private set; }

        public SelectorFrameInput()
        {
            Selecting = false;
            MapCoordinates = new Vector2();
        }

        public void NewPoint(Vector2 mousePos)
        {
            Selecting = true;
            MapCoordinates = mousePos;
        }

        public void EndSelection(Vector2 mousePos)
        {
            Selecting = false;
            MapCoordinates = mousePos;
        }
    }
}
