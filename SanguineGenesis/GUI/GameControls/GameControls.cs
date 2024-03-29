﻿using System;
using System.Collections.Generic;
using System.Linq;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameControls
{
    /// <summary>
    /// Takes player's input and performs corresponding action in the game.
    /// </summary>
    class GameControls
    {
        //State related to the map.
        public MapMovementInput MapMovementInput { get; private set; }
        public MapView MapView { get; private set; }
        public MapSelectorRect MapSelectorRect { get; private set; }

        //State of communication with game logic.
        public SelectionInput SelectionInput { get; private set; }
        public SelectedGroup SelectedGroup { get; private set; }

        /// <summary>
        /// Receives errors during command setting and execution.
        /// </summary>
        public ActionLog ActionLog { get; private set; }

        public GameControls()
        {
            Reset();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            MapView = new MapView(0, 0, 60);
            MapMovementInput = new MapMovementInput();
            SelectionInput = new SelectionInput();
            MapSelectorRect = null;
            SelectedGroup = new SelectedGroup();
            ActionLog = new ActionLog(4);
        }

        /// <summary>
        /// Moves MapView using MapMovementInput.
        /// </summary>
        public void MoveMapView(Map map)
        {
            //move map view only if player isn't currently selecting units
            if (SelectionInput.State != SelectionInputState.SELECTING_ENTITIES)
                foreach (Direction d in MapMovementInput.MapDirection)
                    MapView.Move(d, map);
        }

        /// <summary>
        /// Selects entities based on SelectionInput and MapSelectorRect. Sets commands
        /// to SelectedEntities.
        /// </summary>
        public void UpdateEntitiesByInput(Game game)
        {
            switch (SelectionInput.State)
            {
                case SelectionInputState.SELECTING_ENTITIES:
                case SelectionInputState.FINISH_SELECTING_ENTITIES:
                {
                    Vector2 mapPoint = SelectionInput.SelectingCoordinates;

                    //initialize map selector rectangle
                    if (MapSelectorRect == null)
                    {
                        //create new map selector rectangle
                        MapSelectorRect = new MapSelectorRect(mapPoint);

                        //remove all previously selected entities
                        SelectedGroup.ClearTemporary();

                        //reset selected ability
                        SelectionInput.SelectedAbility = null;
                    }
                        
                    //update selected entities
                    MapSelectorRect.EndPoint = mapPoint;
                    List<Entity> selected = MapSelectorRect.GetSelectedEntities(game).ToList();
                    SelectedGroup.SetTemporaryEntities(selected);

                    //finish selecting
                    if (SelectionInput.State == SelectionInputState.FINISH_SELECTING_ENTITIES)
                    {
                        MapSelectorRect = null;
                        SelectedGroup.CommitEntities();
                        if (SelectedGroup.Entities().Any())
                            SelectionInput.State = SelectionInputState.ENTITIES_SELECTED;
                        else
                            SelectionInput.State = SelectionInputState.IDLE;
                    }
                    break;
                }
                case SelectionInputState.ENTITIES_SELECTED:
                { 
                    Ability selectedAbility = SelectionInput.SelectedAbility;
                    //if the selected ability requires no target, use it
                    if (SelectionInput.IsAbilitySelected 
                        && selectedAbility.TargetType == typeof(Nothing))
                    {
                        //use the ability
                        IEnumerable<Entity> entitiesWithAbil = SelectedGroup.Entities();
                        selectedAbility.SetCommands(entitiesWithAbil, Nothing.Get, SelectionInput.ResetCommandsQueue, ActionLog);
                        //reset ability selection
                        SelectionInput.SelectedAbility = null;
                    }
                    break;
                }
                case SelectionInputState.TARGET_SELECTED:
                {
                    if (!SelectionInput.IsAbilitySelected)
                    //ability wasn't selected, use default abilities
                    {
                        UseDefaultAbility(game, SelectionInput.TargetCoordinates, SelectionInput.ResetCommandsQueue);
                    }
                    else
                    //ability was selected
                    {
                        Ability ability = SelectionInput.SelectedAbility;
                        ITargetable target = FindAbilityTarget(game, ability, SelectionInput.TargetCoordinates);
                        if (target != null)
                        {
                            //use the ability if a valid target was selected
                            IEnumerable<Entity> entitiesWithAbil = SelectedGroup.Entities();
                            ability.SetCommands(entitiesWithAbil, target, SelectionInput.ResetCommandsQueue, ActionLog);
                        }
                        else
                        {
                            ActionLog.LogError(null, ability, "target is not valid");
                        }
                    }

                    //reset ability selection, regargless of success of using selected ability
                    SelectionInput.SelectedAbility = null;
                    SelectionInput.State = SelectionInputState.ENTITIES_SELECTED;
                    break;
                }
            }
            //update selected group
            SelectedGroup.RemoveDead();
        }

        /// <summary>
        /// Make entities use default ability on the target at targetCoords.
        /// </summary>
        /// <param name="resetQueue">True if the entities command queue should be reset.</param>
        private void UseDefaultAbility(Game game, Vector2 targetCoords, bool resetQueue)
        {
            //determine target
            ITargetable enemy = SelectClickedEntityTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer,
                (entity) => entity.Faction.FactionID != game.CurrentPlayer.FactionID, typeof(Entity));

            if (enemy == null)
            {
                //no enemy selected => move to the movement target building that is at clicked coordinates
                var moveToAbility = game.GameData.Abilities.UnbreakableMoveTo;
                ITargetable movementTarget = FindAbilityTarget(game, moveToAbility, SelectionInput.TargetCoordinates);
                //if there is no building at clicked coordinates, move to the coordinates, otherwise move to the building
                if (!(movementTarget is Building))
                    movementTarget = targetCoords;
                moveToAbility.SetCommands(SelectedGroup.Entities()
                    .Where((e) => e.GetType() == typeof(Animal)).Cast<Animal>(), movementTarget, resetQueue, ActionLog.ThrowAway);
            }
            else
            {
                //enemy selected => attack it
                game.GameData.Abilities.UnbreakableAttack.SetCommands(SelectedGroup.Entities(), enemy, resetQueue, ActionLog.ThrowAway);
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
                target = SelectClickedEntityTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, targetType);
                if (target == null)
                    target = targetCoords;
            }
            else if (targetType == typeof(IHerbivoreFood))
            {
                //target is a plant or node
                target = SelectClickedEntityTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, typeof(Plant));
                //there is no plant clicked, so use node
                if (target == null)
                    target = game.Map[(int)targetCoords.X, (int)targetCoords.Y];
            }
            else if (targetType == typeof(ICarnivoreFood))
            {
                //target is a plant or node
                target = SelectClickedEntityTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, typeof(ICarnivoreFood));
            }
            else if (targetType == typeof(Node))
            {
                //target is a node
                target = game.Map[(int)targetCoords.X, (int)targetCoords.Y];
            }
            else if (typeof(Entity).IsAssignableFrom(targetType))
            {
                //target is an entity
                target = SelectClickedEntityTarget(game, targetCoords.X, targetCoords.Y, game.CurrentPlayer, (e) => true, targetType);
            }
            return target;
        }

        /// <summary>
        /// Selects target visible for the selecting player of the given type. Instances of the type have to implement
        /// ITargetable.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the instance of type doesn't implement ITargetable.</exception>
        private ITargetable SelectClickedEntityTarget(Game game, float x, float y, Player selectingPlayer, Func<Entity,bool> condition, Type type)
        {
            if (!typeof(ITargetable).IsAssignableFrom(type))
                throw new ArgumentException("The type has to inherit from ITargetable!");

            IEnumerable<Entity> allEntities = GameQuerying.SelectEntitiesInArea(
                                    game, new Rect(x, y, x, y))
                                    .Where((entity) => type.IsAssignableFrom(entity.GetType()) && condition(entity));
            return GameQuerying.SelectVisibleEntities(selectingPlayer, allEntities).FirstOrDefault();
        }
    }
}
