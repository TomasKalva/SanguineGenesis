using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wpfTest.MainWindow;

namespace wpfTest
{
    class GameControls
    {
        public MapMovementInput MapMovementInput { get; }
        public SelectorFrameInput SelectorFrameInput { get; }
        public MapView MapView { get; }
        public MapSelectorFrame MapSelectorFrame { get; private set; }
        public CommandsGroup SelectedUnits { get; private set; }

        public GameControls(MapView mapView, MapMovementInput mapMovementInput)
        {
            MapView = mapView;
            MapMovementInput = mapMovementInput;
            SelectorFrameInput = new SelectorFrameInput();
            MapSelectorFrame = null;
            SelectedUnits = new CommandsGroup();
        }

        public void ProcessInput(Game game)
        {
            //move map view only if player isn't currently selecting units
            if(!SelectorFrameInput.Selecting)
                foreach (Direction d in MapMovementInput.MapDirection)
                    MapView.Move(d,game.Map);

            if (SelectorFrameInput.Selecting)
            {
                Vector2 mapPoint = SelectorFrameInput.MapCoordinates;
                if (MapSelectorFrame == null)
                {
                    MapSelectorFrame = new MapSelectorFrame(mapPoint);
                    SelectedUnits.RemoveUnits(SelectedUnits.Units);
                }
                else
                {
                    MapSelectorFrame.SetEndPoint(mapPoint);
                    /*foreach(Unit u in MapSelectorFrame.GetSelectedUnits(game))
                    {
                        u.Group = new CommandsGroup();
                    }*/
                    SelectedUnits.SetUnits(MapSelectorFrame.GetSelectedUnits(game));
                    MapSelectorFrame.Update();
                }
            }
            else
            {
                MapSelectorFrame = null;
            }
        }
    }
}
