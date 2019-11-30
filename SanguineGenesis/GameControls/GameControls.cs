using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;

namespace SanguineGenesis
{
    /// <summary>
    /// Takes player's input and performs corresponding action in the game.
    /// </summary>
    class GameControls
    {
        public MapMovementInput MapMovementInput { get; }
        public MapView MapView { get; }

        public EntityCommandsInput EntityCommandsInput { get; }
        public MapSelectorFrame MapSelectorFrame { get; set; }
        public SelectedGroup SelectedGroup { get; }


        public GameControls(Map map)
        {
            MapView = new MapView(0, 0, 60, map);
            MapMovementInput = new MapMovementInput();
            EntityCommandsInput = new EntityCommandsInput();
            MapSelectorFrame = null;
            SelectedGroup = new SelectedGroup();
        }

        /// <summary>
        /// Has to be called from the window thread.
        /// </summary>
        public void UpdateMapView(Map map)
        {
            //move map view only if player isn't currently selecting units
            if (EntityCommandsInput.State != EntityCommandsInputState.SELECTING_UNITS)
                foreach (Direction d in MapMovementInput.MapDirection)
                    MapView.Move(d, map);
        }

        /// <summary>
        /// Selects entities based on EntityCommandsInput and MapSelector frame. Sets commands
        /// to SelectedEntities.
        /// </summary>
        public void UpdateEntitiesByInput(Game game)
        {
            switch (EntityCommandsInput.State)
            {
                case EntityCommandsInputState.SELECTING_UNITS:
                case EntityCommandsInputState.FINISH_SELECTING_UNITS:
                    {
                        Vector2 mapPoint = EntityCommandsInput.SelectingCoordinates;

                        //initialize map selector frame
                        if (MapSelectorFrame == null)
                        {
                            //create new map selector frame
                            MapSelectorFrame = new MapSelectorFrame(mapPoint);

                            //remove all previously selected entities
                            SelectedGroup.ClearTemporary();

                            //reset selected ability
                            EntityCommandsInput.SelectedAbility = null;
                        }
                        
                        //update selected entities
                        MapSelectorFrame.SetEndPoint(mapPoint);
                        MapSelectorFrame.Update();
                        List<Entity> selected = MapSelectorFrame.GetSelectedUnits(game).ToList();
                        SelectedGroup.SetEntities(selected);

                        //finish selecting
                        if (EntityCommandsInput.State == EntityCommandsInputState.FINISH_SELECTING_UNITS)
                        {
                            MapSelectorFrame = null;
                            if (SelectedGroup.Entities.Any())
                            {
                                SelectedGroup.CommitEntities();
                                SelectedGroup.SortEntities();
                                EntityCommandsInput.State = EntityCommandsInputState.UNITS_SELECTED;
                            }
                            else
                                EntityCommandsInput.State = EntityCommandsInputState.IDLE;
                        }
                        break;
                    }
                case EntityCommandsInputState.UNITS_SELECTED:
                    { 
                        Ability selectedAbility = EntityCommandsInput.SelectedAbility;
                        //if the selected ability requires no target, use it
                        if (EntityCommandsInput.IsAbilitySelected 
                            && selectedAbility.TargetType == typeof(Nothing))
                        {
                            //use the ability
                            IEnumerable<Entity> entitiesWithAbil = SelectedGroup.Entities.Where((e) => e.Abilities.Contains(selectedAbility));
                            if (entitiesWithAbil != null)
                            {
                                selectedAbility.SetCommands(entitiesWithAbil, Nothing.Get, EntityCommandsInput.ResetCommandsQueue);
                            }
                            //reset ability selection
                            EntityCommandsInput.SelectedAbility = null;
                        }
                        break;
                    }
                case EntityCommandsInputState.ABILITY_TARGET_SELECTED:
                    {
                        if (!EntityCommandsInput.IsAbilitySelected)
                        //ability wasn't selected, use default abilities
                        {
                            UseDefaultAbility(game, EntityCommandsInput.TargetCoordinates, EntityCommandsInput.ResetCommandsQueue);
                        }
                        else
                        //ability was selected
                        {
                            Ability ability = EntityCommandsInput.SelectedAbility;
                            ITargetable target=FindAbilityTarget(game, ability, EntityCommandsInput.TargetCoordinates);
                            if (target != null)
                            {
                                //use the ability if a valid target was selected
                                IEnumerable<Entity> entitiesWithAbil = SelectedGroup.Entities.Where((e) => e.Abilities.Contains(ability));
                                ability.SetCommands(entitiesWithAbil, target, EntityCommandsInput.ResetCommandsQueue);
                            }
                        }

                        //reset ability selection, regargless of success of using selected ability
                        EntityCommandsInput.SelectedAbility = null;
                        EntityCommandsInput.State = EntityCommandsInputState.UNITS_SELECTED;
                        break;
                    }
            }
            SelectedGroup.RemoveDead();
        }

        /// <summary>
        /// Make entities use default ability on the target at targetCoords.
        /// </summary>
        /// <param name="resetQueue">True if the entities command queue should be reset.</param>
        private void UseDefaultAbility(Game game, Vector2 targetCoords, bool resetQueue)
        {
            //determine target
            ITargetable enemy = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer,
                (entity) => entity.Faction.FactionID != game.CurrentPlayer.FactionID, typeof(Entity));

            if (enemy == null)
            {
                //no enemy selected, move to the clicked coordiantes
                game.CurrentPlayer.GameStaticData.Abilities.MoveTo.SetCommands(SelectedGroup.Entities
                    .Where((e) => e.GetType() == typeof(Animal)).Cast<Animal>(), targetCoords, resetQueue);
            }
            else
            {
                //enemy selected => attack it
                game.CurrentPlayer.GameStaticData.Abilities.Attack.SetCommands(SelectedGroup.Entities, enemy, resetQueue);
            }
        }

        /// <summary>
        /// Select valid target for the ability.
        /// </summary>
        private ITargetable FindAbilityTarget(Game game, Ability ability, Vector2 targetCoords)
        {
            //if no target is found, the target doesn't exist
            ITargetable target=null;
            //type of target of the selected ability
            Type targetType = ability.TargetType;
            if (targetType == typeof(Vector2))
            {
                //target is a vector
                target = targetCoords;
            }
            else if (targetType == typeof(IMovementTarget))
            {
                //target is a movement target
                target = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, targetType);
                if (target == null)
                    target = targetCoords;
            }
            else if (targetType == typeof(IHerbivoreFood))
            {
                //target is a tree or node
                target = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, typeof(Tree));
                //there is no tree clicked, so use node
                if (target == null)
                    target = game.Map[(int)targetCoords.X, (int)targetCoords.Y];
            }
            else if (targetType == typeof(ICarnivoreFood))
            {
                //target is a tree or node
                target = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, typeof(ICarnivoreFood));
            }
            else if (targetType == typeof(Node))
            {
                //target is a node
                target = game.Map[(int)targetCoords.X, (int)targetCoords.Y];
            }
            else if (typeof(Entity).IsAssignableFrom(targetType))
            {
                //target is an entity
                target = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, targetType);
            }
            return target;
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

            IEnumerable<Entity> allEntities = GameQuerying.SelectRectEntities(
                                    game, new Rect(x, y, x, y),
                                    (entity) => type.IsAssignableFrom(entity.GetType()) && condition(entity))
                                    .ToList();
            return GameQuerying.SelectVisibleEntities(selectingPlayer, allEntities).FirstOrDefault();
        }
    }
}
