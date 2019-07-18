using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class Ability
    {
        public AbilityType AbilityType { get; }

        public Ability(AbilityType abilityType)
        {
            AbilityType = abilityType;
        }

    }

    public class TargetPointAbility:Ability
    {
        public float Distance { get; }

        public TargetPointAbility(AbilityType abilityType, float distance)
            :base(abilityType)
        {
            Distance = distance;
        }

        /// <summary>
        /// Assigns a new instance of the command assignment for this ability and returns it.
        /// </summary>
        public void AssignCommands(List<Unit> units, Vector2 target, Game game)
        {
            //move to the target until the minimal distance is reached
            if(!(AbilityType==AbilityType.MOVE_TO) && !(AbilityType==AbilityType.MOVE_TOWARDS))
                AssignMovementTo(units, target, Distance, game);

            switch (AbilityType)
            {
                case AbilityType.MOVE_TO:
                    {
                        //first use flowmap to navigate close to the point
                        MoveToCommandAssignment mto = new MoveToCommandAssignment(units, target);
                        mto.Process(game);
                        mto.AssignCommands();
                        //then move straight towards the point
                        MoveTowardsCommandAssignment mtow = new MoveTowardsCommandAssignment(units, target);
                        mtow.Process(game);
                        mtow.AssignCommands();
                        return;
                    }
                case AbilityType.MOVE_TOWARDS:
                    {
                        //move straight towards the point
                        MoveTowardsCommandAssignment mtow = new MoveTowardsCommandAssignment(units, target);
                        mtow.Process(game);
                        mtow.AssignCommands();
                        return;
                    }
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");
            
        }

        /// <summary>
        /// Assigns commands to units to move to the target until they are at most the endDistance
        /// from it
        /// </summary>
        /// <param name="endDistance">Distance to the target when the unit stops moving.</param>
        private void AssignMovementTo(List<Unit> units, Vector2 target, float endDistance, Game game)
        {
            MoveToCommandAssignment mto=new MoveToCommandAssignment(units, target, endDistance);
            mto.Process(game);
            mto.AssignCommands();
            MoveTowardsCommandAssignment mtow = new MoveTowardsCommandAssignment(units, target, endDistance);
            mtow.Process(game);
            mtow.AssignCommands();
        }
    }

    public class TargetUnitAbility : Ability
    {
        public float Distance { get; }

        public TargetUnitAbility(AbilityType abilityType, float distance)
            : base(abilityType)
        {
            Distance = distance;
        }

        /// <summary>
        /// Assigns a new instance of the command assignment for this ability and returns it.
        /// </summary>
        public void AssignCommands(List<Unit> units, Unit target, Game game)
        {
            //move to the target until the minimal distance is reached
            if (!(AbilityType == AbilityType.MOVE_TO) && !(AbilityType == AbilityType.MOVE_TOWARDS))
                AssignMovementTo(units, target.Pos, Distance, game);

            switch (AbilityType)
            {
                case AbilityType.ATTACK:
                    AttackCommandAssignment mto = new AttackCommandAssignment(units, target);
                    mto.Process(game);
                    mto.AssignCommands();
                    return;
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");

        }

        /// <summary>
        /// Assigns commands to units to move to the target until they are at most the endDistance
        /// from it
        /// </summary>
        /// <param name="endDistance">Distance to the target when the unit stops moving.</param>
        private void AssignMovementTo(List<Unit> units, Vector2 target, float endDistance, Game game)
        {
            MoveToCommandAssignment mto = new MoveToCommandAssignment(units, target, endDistance);
            mto.Process(game);
            mto.AssignCommands();
            MoveTowardsCommandAssignment mtow = new MoveTowardsCommandAssignment(units, target, endDistance);
            mtow.Process(game);
            mtow.AssignCommands();
        }
    }

    public enum AbilityType
    {
        MOVE_TO,
        MOVE_TOWARDS,
        ATTACK
    }
}
