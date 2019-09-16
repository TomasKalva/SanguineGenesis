using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SanguineGenesis.GameLogic;

namespace SanguineGenesis
{
    /// <summary>
    /// 
    /// </summary>
    class EntityCommandsInput
    {
        //immutable
        public Vector2 INVALID_MAP_COORDINATES => new Vector2(-1, -1);
        private Dictionary<Key, Ability> keyToAbility;

        //lock is this instance
        public EntityCommandsInputState State { get; set; }
        public Vector2 SelectingCoordinates { get; private set; }
        public Vector2 TargetCoordinates { get; private set; }
        public Entity TargetedEntity { get; private set; }
        private Ability selectedAbility;
        public Ability SelectedAbility { get { return selectedAbility; } set { selectedAbility = value; IsAbilitySelected = true; } }
        public bool IsAbilitySelected { get; set; }
        /// <summary>
        /// True iff before setting new commands, the old should be removed.
        /// </summary>
        public bool ResetCommandsQueue { get; set; }

        internal EntityCommandsInput(Game game)
        {
            State = EntityCommandsInputState.IDLE;
            SelectingCoordinates = new Vector2();
            SelectedAbility = game.CurrentPlayer.GameStaticData.Abilities.MoveTo;
            IsAbilitySelected = false;
            ResetCommandsQueue = true;

            //initialize keyToAbilityType
            keyToAbility = new Dictionary<Key, Ability>();
            keyToAbility.Add(Key.Escape, game.CurrentPlayer.GameStaticData.Abilities.MoveTo);
        }

        /// <summary>
        /// Set new corner for selection frame.
        /// </summary>
        public void NewPoint(Vector2 mousePos)
        {
            lock (this)
            { 
                State = EntityCommandsInputState.SELECTING_UNITS;
                SelectingCoordinates = mousePos;
            }
        }

        /// <summary>
        /// Set new corner for selection frame and end selecting.
        /// </summary>
        public void EndSelection(Vector2 mousePos)
        {
            lock (this)
            {
                State = EntityCommandsInputState.UNITS_SELECTED;
                SelectingCoordinates = mousePos;
            }
        }

        /// <summary>
        /// Sets target point for selecting target for the selected ability.
        /// </summary>
        public void SetTarget(Vector2 mousePos)
        {
            lock (this)
            {
                if (State == EntityCommandsInputState.UNITS_SELECTED)
                {
                    TargetCoordinates = mousePos;
                    State = EntityCommandsInputState.ABILITY_TARGET_SELECTED;
                }
            }
        }
    }

    /// <summary>
    /// States of EntityCommandsInput.
    /// </summary>
    public enum EntityCommandsInputState
    {
        IDLE,
        SELECTING_UNITS,
        UNITS_SELECTED,
        ABILITY_TARGET_SELECTED
    }
}
