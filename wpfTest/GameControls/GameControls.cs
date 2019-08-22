using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Abilities;
using static wpfTest.MainWindow;

namespace wpfTest
{
    class GameControls
    {
        public MapMovementInput MapMovementInput { get; }
        public UnitCommandsInput UnitCommandsInput { get; }
        public MapView MapView { get; }
        public MapSelectorFrame MapSelectorFrame { get; set; }
        public CommandsGroup SelectedEntities { get; private set; }
        

        public GameControls(MapView mapView, MapMovementInput mapMovementInput, Game game)
        {
            MapView = mapView;
            MapMovementInput = mapMovementInput;
            UnitCommandsInput = new UnitCommandsInput(game);
            MapSelectorFrame = null;
            SelectedEntities = new CommandsGroup();
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
                    {
                        Vector2 mapPoint = UnitCommandsInput.MapCoordinates;
                        if (MapSelectorFrame == null)
                        {
                            MapSelectorFrame = new MapSelectorFrame(mapPoint);
                            SelectedEntities.RemoveUnits(SelectedEntities.Entities);
                            //reset selected ability
                            lock (UnitCommandsInput)
                            {
                                UnitCommandsInput.AbilitySelected = false;
                            }
                        }
                        else
                        {
                            MapSelectorFrame.SetEndPoint(mapPoint);
                            SelectedEntities.SetUnits(MapSelectorFrame.GetSelectedUnits(game).ToList());
                            MapSelectorFrame.Update();
                        }
                        break;
                    }
                case UnitsCommandInputState.SELECTED:
                    { 
                        //remove map selector frame
                        if (MapSelectorFrame != null)
                            MapSelectorFrame = null;

                        //set rally points for buildings
                        /*Vector2 clickCoords = UnitCommandsInput.MapCoordinates;
                        if (clickCoords != UnitCommandsInput.INVALID_MAP_COORDINATES)
                        foreach(Building building in SelectedEntities.Entities.Where((e)=>e is Building))
                        {
                                building.RallyPoint = clickCoords;
                        }*/

                        //use ability that doesn't require target
                        Ability noTargetAbility = null;
                        bool selectedNoTargetAbility = false;
                        lock (UnitCommandsInput)
                        {
                            if (UnitCommandsInput.AbilitySelected)
                            {
                                noTargetAbility = UnitCommandsInput.Ability;
                                selectedNoTargetAbility = (noTargetAbility != null) && noTargetAbility.TargetType == typeof(Nothing);
                                if (selectedNoTargetAbility)
                                    UnitCommandsInput.AbilitySelected = false;
                            }
                        }
                        if (selectedNoTargetAbility)
                        {
                            //use the ability with no target
                            IEnumerable<Entity> unitsWithAbil = SelectedEntities.Entities.Where((e) => e.Abilities.Contains(noTargetAbility));
                            if (unitsWithAbil != null)
                            {
                                noTargetAbility.SetCommands(unitsWithAbil, Nothing.Get);
                            }
                        }
                        break;
                    }
                case UnitsCommandInputState.ABILITY:
                    { 
                        //determine target
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
                        //ability wasn't selected => use default abilities
                        {
                            //determine target
                            ITargetable enemy = SelectClickedTarget(game, clickCoords.X, clickCoords.Y, game.CurrentPlayer, 
                                (entity) => entity.Player.PlayerID != game.CurrentPlayer.PlayerID, typeof(Entity));

                            if (enemy == null)
                            {
                                //no enemy selected, move to the clicked coordiantes
                                game.CurrentPlayer.GameStaticData.Abilities.MoveTo.SetCommands(SelectedEntities.Entities
                                    .Where((e)=>e.GetType()==typeof(Animal)).Cast<Animal>(), clickCoords);
                            }
                            else
                            {
                                //enemy selected => attack it
                               game.CurrentPlayer.GameStaticData.Abilities.Attack.SetCommands(SelectedEntities.Entities, enemy);
                            }
                        }
                        else
                        //ability was selected
                        {
                            //determine target
                            ITargetable targ=null;
                            //type of target of the selected ability
                            Type targetType = abil.TargetType;
                            if(targetType == typeof(Vector2))
                            {
                                //target is a vector
                                targ = clickCoords;
                            }
                            else if(targetType == typeof(IMovementTarget))
                            {
                                //target is a movement target
                                targ= SelectClickedTarget(game, clickCoords.X, clickCoords.Y, game.CurrentPlayer, (e) => true, targetType);
                                if (targ == null)
                                    targ = clickCoords;
                            }
                            else if (targetType == typeof(IHerbivoreFood))
                            {
                                //target is a tree or node
                                targ = SelectClickedTarget(game, clickCoords.X, clickCoords.Y, game.CurrentPlayer, (e) => true, typeof(Tree));
                                //there is no tree clicked, so use node
                                if (targ==null)
                                    targ = game.Map[(int)clickCoords.X, (int)clickCoords.Y];
                            }
                            else if (targetType == typeof(ICarnivoreFood))
                            {
                                //target is a tree or node
                                targ = SelectClickedTarget(game, clickCoords.X, clickCoords.Y, game.CurrentPlayer, (e) => true, typeof(ICarnivoreFood));
                            }
                            else if (targetType == typeof(Node))
                            {
                                //target is a node
                                targ = game.Map[(int)clickCoords.X, (int)clickCoords.Y];
                            }
                            else if (typeof(Entity).IsAssignableFrom(targetType))
                            {
                                //target is an entity
                                targ = SelectClickedTarget(game, clickCoords.X, clickCoords.Y, game.CurrentPlayer, (e) => true, targetType);
                            }

                            if (targ != null)
                            {
                                //use the ability if a valid target was selected
                                IEnumerable<Entity> unitsWithAbil = SelectedEntities.Entities.Where((e) => e.Abilities.Contains(abil));
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
            }
            SelectedEntities.RemoveDead();
        }

        /// <summary>
        /// Selects target visible for the selecting player of the given type. Instances of the type have to implement
        /// ITargetable.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if instances of type don't implement ITargetable.</exception>
        private ITargetable SelectClickedTarget(Game game, float x, float y, Player selectingPlayer, Func<Entity,bool> condition, Type type)
        {
            if (!typeof(ITargetable).IsAssignableFrom(type))
                throw new ArgumentException("The type has to inherit from ITargetable!");

            IEnumerable<Entity> allEntities = GameQuerying.GetGameQuerying().SelectRectEntities(
                                    game, new Rect(x, y, x, y),
                                    (entity) => type.IsAssignableFrom(entity.GetType()) && condition(entity))
                                    .ToList();
            return GameQuerying.GetGameQuerying().SelectVisibleEntities(game, selectingPlayer, allEntities).FirstOrDefault();
        }
    }
}
