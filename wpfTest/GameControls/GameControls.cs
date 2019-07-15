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
        public UnitCommandsInput UnitCommandsInput { get; }
        public MapView MapView { get; }
        public MapSelectorFrame MapSelectorFrame { get; set; }
        public CommandsGroup SelectedUnits { get; private set; }
        

        public GameControls(MapView mapView, MapMovementInput mapMovementInput)
        {
            MapView = mapView;
            MapMovementInput = mapMovementInput;
            UnitCommandsInput = new UnitCommandsInput();
            MapSelectorFrame = null;
            SelectedUnits = new CommandsGroup();
        }

        /// <summary>
        /// Has to be called from the window thread.
        /// </summary>
        public void ProcessInput(Game game)
        {
                //move map view only if player isn't currently selecting units
                if (UnitCommandsInput.State != UnitsCommandInputState.SELECTING)
                    foreach (Direction d in MapMovementInput.MapDirection)
                        MapView.Move(d, game.Map);
        }

        /// <summary>
        /// Has to be called from the main loop of the game, with game being locked.
        /// </summary>
        public void UpdateUnitsByInput(Game game)
        {
            switch (UnitCommandsInput.State)
            {
                case UnitsCommandInputState.SELECTING:
                    Vector2 mapPoint = UnitCommandsInput.MapCoordinates;
                    if (MapSelectorFrame == null)
                    {
                        MapSelectorFrame = new MapSelectorFrame(mapPoint);
                        SelectedUnits.RemoveUnits(SelectedUnits.Units);
                    }
                    else
                    {
                        MapSelectorFrame.SetEndPoint(mapPoint);
                        SelectedUnits.SetUnits(MapSelectorFrame.GetSelectedUnits(game));
                        MapSelectorFrame.Update();
                    }
                    break;
                case UnitsCommandInputState.SELECTED:
                    MapSelectorFrame = null;
                    break;
                case UnitsCommandInputState.ABILITY:
                    //SelectedUnits.SetCommand(new MoveTowardsCommandFactory(UnitCommandsInput.MapCoordinates));
                    SelectedUnits.SetCommand(new MoveToCommandFactory(UnitCommandsInput.MapCoordinates,game));
                    //setting state from this thread can cause inconsistency of State
                    //todo: maybe encode states into byte - operations should be atomic => no inconsistent state
                    UnitCommandsInput.State = UnitsCommandInputState.SELECTED;
                    break;
            }
        }
    }
}
