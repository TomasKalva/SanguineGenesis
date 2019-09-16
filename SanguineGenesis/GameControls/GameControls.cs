using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using static SanguineGenesis.MainWindow;

namespace SanguineGenesis
{
    /// <summary>
    /// Takes player's input and performs corresponding action in the game.
    /// </summary>
    class GameControls
    {
        public MapMovementInput MapMovementInput { get; }
        public MapView MapView { get; }

        public MapSelectorFrame MapSelectorFrame { get; set; }

        public EntityCommandsInput EntityCommandsInput { get; }
        public SelectedGroup SelectedEntities { get; }


        public GameControls(MapView mapView, Game game)
        {
            MapView = mapView;
            MapMovementInput = new MapMovementInput();
            EntityCommandsInput = new EntityCommandsInput(game);
            MapSelectorFrame = null;
            SelectedEntities = new SelectedGroup();
        }

        /// <summary>
        /// Has to be called from the window thread.
        /// </summary>
        public void UpdateMapView(Game game)
        {
            //move map view only if player isn't currently selecting units
            if (EntityCommandsInput.State != EntityCommandsInputState.SELECTING_UNITS)
                foreach (Direction d in MapMovementInput.MapDirection)
                    MapView.Move(d, game.Map);
        }

        /// <summary>
        /// Selects entities based on EntityCommandsInput and MapSelector frame. Sets commands
        /// to SelectedEntities.
        /// Has to be called from the main loop of the game, with game being locked.
        /// </summary>
        public void UpdateEntitiesByInput(Game game)
        {

            switch (EntityCommandsInput.State)
            {
                case EntityCommandsInputState.SELECTING_UNITS:
                    {
                        Vector2 mapPoint;
                        lock (EntityCommandsInput)
                        {
                            mapPoint = EntityCommandsInput.SelectingCoordinates;
                            if(MapSelectorFrame==null)
                                //reset selected ability
                                EntityCommandsInput.IsAbilitySelected = false;
                        }

                        if (MapSelectorFrame == null)
                        {
                            MapSelectorFrame = new MapSelectorFrame(mapPoint);
                            SelectedEntities.Clear();
                        }
                        else
                        {
                            MapSelectorFrame.SetEndPoint(mapPoint);
                            List<Entity> selected = MapSelectorFrame.GetSelectedUnits(game).ToList();
                            SelectedEntities.SetEntities(selected);
                            MapSelectorFrame.Update();
                        }
                        break;
                    }
                case EntityCommandsInputState.UNITS_SELECTED:
                    { 
                        //remove map selector frame
                        if (MapSelectorFrame != null)
                            MapSelectorFrame = null;

                        //use ability that doesn't require target
                        Ability noTargetAbility = null;
                        bool selectedNoTargetAbility = false;
                        bool resetCommandsQueue=false;
                        lock (EntityCommandsInput)
                        {
                            if (EntityCommandsInput.IsAbilitySelected)
                            {
                                noTargetAbility = EntityCommandsInput.SelectedAbility;
                                selectedNoTargetAbility = (noTargetAbility != null) && noTargetAbility.TargetType == typeof(Nothing);
                                resetCommandsQueue = EntityCommandsInput.ResetCommandsQueue;
                                //reset ability selection
                                if (selectedNoTargetAbility)
                                    EntityCommandsInput.IsAbilitySelected = false;
                            }
                        }
                        if (selectedNoTargetAbility)
                        {
                            //use the ability with no target
                            IEnumerable<Entity> entitiesWithAbil = SelectedEntities.Entities.Where((e) => e.Abilities.Contains(noTargetAbility));
                            if (entitiesWithAbil != null)
                            {
                                noTargetAbility.SetCommands(entitiesWithAbil, Nothing.Get, resetCommandsQueue);
                            }
                        }
                        break;
                    }
                case EntityCommandsInputState.ABILITY_TARGET_SELECTED:
                    {

                        //get information about selected ability and target
                        Vector2 targetCoords;
                        bool isAbilitySelected;
                        Ability ability;
                        bool resetCommandsQueue;
                        lock (EntityCommandsInput)
                        {
                            targetCoords = EntityCommandsInput.TargetCoordinates;
                            isAbilitySelected = EntityCommandsInput.IsAbilitySelected;
                            ability = EntityCommandsInput.SelectedAbility;
                            resetCommandsQueue = EntityCommandsInput.ResetCommandsQueue;

                            //reset ability selection, regargless of success of using selected ability
                            EntityCommandsInput.IsAbilitySelected = false;
                            EntityCommandsInput.State = EntityCommandsInputState.UNITS_SELECTED;
                        }

                        if (!isAbilitySelected)
                        //ability wasn't selected, use default abilities
                        {
                            UseDefaultAbility(game, targetCoords, resetCommandsQueue);
                        }
                        else
                        //ability was selected
                        {
                            //determine target
                            ITargetable target=FindAbilityTarget(game, ability,targetCoords);

                            if (target != null)
                            {
                                //use the ability if a valid target was selected
                                IEnumerable<Entity> entitiesWithAbil = SelectedEntities.Entities.Where((e) => e.Abilities.Contains(ability));
                                if (entitiesWithAbil != null)
                                {
                                    ability.SetCommands(entitiesWithAbil, target, resetCommandsQueue);
                                }
                            }
                        }
                        break;
                    }
            }
            SelectedEntities.RemoveDead();
        }

        /// <summary>
        /// Use default ability by entities on with the targetCoords.
        /// </summary>
        /// <param name="resetQueue">True if the entities command queue should be reset.</param>
        private void UseDefaultAbility(Game game, Vector2 targetCoords, bool resetQueue)
        {
            //determine target
            ITargetable enemy = SelectClickedTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer,
                (entity) => entity.Player.PlayerID != game.CurrentPlayer.PlayerID, typeof(Entity));

            if (enemy == null)
            {
                //no enemy selected, move to the clicked coordiantes
                game.CurrentPlayer.GameStaticData.Abilities.MoveTo.SetCommands(SelectedEntities.Entities
                    .Where((e) => e.GetType() == typeof(Animal)).Cast<Animal>(), targetCoords, resetQueue);
            }
            else
            {
                //enemy selected => attack it
                game.CurrentPlayer.GameStaticData.Abilities.Attack.SetCommands(SelectedEntities.Entities, enemy, resetQueue);
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

            IEnumerable<Entity> allEntities = GameQuerying.GetGameQuerying().SelectRectEntities(
                                    game, new Rect(x, y, x, y),
                                    (entity) => type.IsAssignableFrom(entity.GetType()) && condition(entity))
                                    .ToList();
            return GameQuerying.GetGameQuerying().SelectVisibleEntities(game, selectingPlayer, allEntities).FirstOrDefault();
        }
    }
}
