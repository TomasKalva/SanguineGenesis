using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class UnitCommandsInput
    {
        //public bool Selecting { get; private set; }
        public UnitsCommandInputState State { get; set; }
        public Vector2 MapCoordinates { get; private set; }

        public UnitCommandsInput()
        {
            //Selecting = false;
            State = UnitsCommandInputState.IDLE;
            MapCoordinates = new Vector2();
        }

        public void NewPoint(Vector2 mousePos)
        {
            //Selecting = true;
            State = UnitsCommandInputState.SELECTING;
            MapCoordinates = mousePos;
        }

        public void EndSelection(Vector2 mousePos)
        {
            //Selecting = false;
            State = UnitsCommandInputState.SELECTED;
            MapCoordinates = mousePos;
        }

        public void SetTarget(Vector2 mousePos)
        {
            if(State==UnitsCommandInputState.SELECTED)
            {
                MapCoordinates = mousePos;
                State = UnitsCommandInputState.ABILITY;
            }
        }
    }

    public enum UnitsCommandInputState
    {
        IDLE,
        SELECTING,
        SELECTED,
        ABILITY
    }
}
