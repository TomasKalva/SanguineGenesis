using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
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
            UnitCommandsInput = UnitCommandsInput.GetUnitCommandsInput();
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
                        //reset selected ability
                        lock (UnitCommandsInput)
                        {
                            UnitCommandsInput.AbilitySelected = false;
                        }
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
                    //find target
                    Vector2 clickCoords = UnitCommandsInput.MapCoordinates;
                    
                    //get information about selected ability
                    bool abilitySelected;
                    Ability abil;
                    lock (UnitCommandsInput)
                    {
                        abilitySelected = UnitCommandsInput.AbilitySelected;
                        abil = UnitCommandsInput.Ability;
                        UnitCommandsInput.AbilitySelected = false;
                    }

                    if (!abilitySelected)
                    {
                        //determine target
                        ITargetable enemy = GameQuerying.GetGameQuerying().SelectRectEntities(
                            game, new Rect(clickCoords.X, clickCoords.Y, clickCoords.X, clickCoords.Y), 
                            (unit) => unit.Player.PlayerID!=game.CurrentPlayer.PlayerID)//don't attack own units
                            .FirstOrDefault();

                        if (enemy == null)
                        {
                            MoveTo.Get.SetCommands(SelectedUnits.Units, clickCoords);
                        }
                        else
                        {
                            Attack.Get.SetCommands(SelectedUnits.Units, enemy);
                        }
                    }
                    else
                    {
                        //determine target
                        ITargetable targ=null;
                        Type targetType = abil.TargetType;
                        if(targetType == typeof(Vector2) ||
                            targetType == typeof(IMovementTarget))
                        {
                            targ = clickCoords;
                        }
                        else if (targetType == typeof(Unit))
                        {
                            targ = GameQuerying.GetGameQuerying().SelectRectEntities(
                                    game, new Rect(clickCoords.X, clickCoords.Y, clickCoords.X, clickCoords.Y), 
                                    (entity) => entity.GetType()==typeof(Unit))
                                    .FirstOrDefault();
                        }
                        else if (targetType == typeof(Building))
                        {
                            targ = GameQuerying.GetGameQuerying().SelectRectEntities(
                                    game, new Rect(clickCoords.X, clickCoords.Y, clickCoords.X, clickCoords.Y),
                                    (entity) => entity.GetType() == typeof(Unit))
                                    .FirstOrDefault();
                        }
                        else if (targetType == typeof(Entity))
                        {
                            targ = GameQuerying.GetGameQuerying().SelectRectEntities(
                                    game, new Rect(clickCoords.X, clickCoords.Y, clickCoords.X, clickCoords.Y),
                                    (entity) => true)
                                    .FirstOrDefault();
                        }
                        else if (targetType == typeof(Node))
                        {
                            targ = game.Map[(int)clickCoords.X, (int)clickCoords.Y];
                        }

                        if (targ != null)
                        {
                            IEnumerable<Entity> unitsWithAbil = SelectedUnits.Units.Where((e) => e.Abilities.Contains(abil));
                            if (unitsWithAbil != null)
                            {
                                abil.SetCommands(unitsWithAbil, targ);
                            }
                        }
                    }
                    //setting state from this thread can cause inconsistency of State
                    //todo: maybe encode states into byte - operations should be atomic => no inconsistent state
                    UnitCommandsInput.State = UnitsCommandInputState.SELECTED;
                    break;
            }
            SelectedUnits.RemoveDead();
        }
    }
}
