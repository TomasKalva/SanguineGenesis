﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameControls
{
    class SelectionInput
    {
        public SelectionInputState State { get; set; }
        public Vector2 SelectingCoordinates { get; private set; }
        public Vector2 TargetCoordinates { get; private set; }
        public Entity TargetedEntity { get; private set; }
        public Ability SelectedAbility { get; set; }
        public bool IsAbilitySelected => SelectedAbility != null;
        /// <summary>
        /// True iff before setting new commands, the old should be removed.
        /// </summary>
        public bool ResetCommandsQueue { get; set; }

        internal SelectionInput()
        {
            State = SelectionInputState.IDLE;
            SelectingCoordinates = new Vector2();
            ResetCommandsQueue = true;
        }

        /// <summary>
        /// Set new corner for selection frame.
        /// </summary>
        public void NewPoint(Vector2 mousePos)
        {
            State = SelectionInputState.SELECTING_UNITS;
            SelectingCoordinates = mousePos;
        }

        /// <summary>
        /// Set new corner for selection frame and end selecting.
        /// </summary>
        public void EndSelection(Vector2 mousePos)
        {
            State = SelectionInputState.FINISH_SELECTING_UNITS;
            SelectingCoordinates = mousePos;
        }

        /// <summary>
        /// Sets target point for selecting target for the selected ability.
        /// </summary>
        public void SetTarget(Vector2 mousePos)
        {
            if (State == SelectionInputState.UNITS_SELECTED)
            {
                TargetCoordinates = mousePos;
                State = SelectionInputState.ABILITY_TARGET_SELECTED;
            }
        }
    }

    /// <summary>
    /// States of EntityCommandsInput.
    /// </summary>
    public enum SelectionInputState
    {
        IDLE,
        SELECTING_UNITS,
        FINISH_SELECTING_UNITS,
        UNITS_SELECTED,
        ABILITY_TARGET_SELECTED
    }
}