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
        /// <summary>
        /// Minimal distance for using this ability.
        /// </summary>
        public float Distance { get; }
        /// <summary>
        /// True if attack distance of the unit should be used instead of Distance.
        /// </summary>
        public bool UsesAttackDistance { get; }

        public Ability(AbilityType abilityType, float distance, bool usesAttackDistance)
        {
            AbilityType = abilityType;
            Distance = distance;
            UsesAttackDistance = usesAttackDistance;
        }

        /// <summary>
        /// Assigns commands to units to move to the target until they are at most the goalDistance
        /// from it
        /// </summary>
        /// <param name="endDistance">Distance to the target when the unit stops moving.</param>
        protected void AssignMovementTo(Players player, List<Unit> units, Vector2 target, Game game)
        {
            //create new instace of move to command assignment and set it to all units
            MoveToCommandAssignment mto = new MoveToCommandAssignment(player, units.ToList(), target, Movement.GROUND, Distance, UsesAttackDistance);
            mto.AssignCommands();
            //add the command assignment to movement generator to create the flowmap asynchronously
            MovementGenerator.GetMovementGenerator().AddNewCommand(Players.PLAYER0, mto);
        }
    }

    public class TargetPointAbility:Ability
    {
        public TargetPointAbility(AbilityType abilityType, float distance, bool usesAttackDistance=false)
            :base(abilityType,distance, usesAttackDistance)
        { }

        /// <summary>
        /// Assigns a new instance of the command assignment for this ability and returns it.
        /// </summary>
        public void AssignCommands(Players player, List<Unit> units, Vector2 target, Game game)
        {
            //move to the target until the minimal distance is reached
            if(!(AbilityType==AbilityType.MOVE_TO))
                AssignMovementTo(player, units, target, game);

            switch (AbilityType)
            {
                case AbilityType.MOVE_TO:
                    {
                        //create new instace of move to command assignment and set it to all units
                        MoveToCommandAssignment mto = new MoveToCommandAssignment(player, units.ToList(), target, Movement.GROUND, Distance, UsesAttackDistance);
                        mto.AssignCommands();
                        //add the command assignment to movement generator to create the flowmap asynchronously
                        MovementGenerator.GetMovementGenerator().AddNewCommand(Players.PLAYER0,mto);
                        return;
                    }
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");
        }
    }

    public class TargetUnitAbility : Ability
    {

        public TargetUnitAbility(AbilityType abilityType, float distance, bool usesAttackDistance = false)
            : base(abilityType,distance, usesAttackDistance)
        { }

        /// <summary>
        /// Assigns a new instance of the command assignment for this ability and returns it.
        /// </summary>
        public void AssignCommands(Players player, List<Unit> units, Unit target, Game game)
        {
            //move to the target until the minimal distance is reached
            if (!(AbilityType == AbilityType.MOVE_TO))
                AssignMovementTo(player, units, target.Pos, game);

            switch (AbilityType)
            {
                case AbilityType.ATTACK:
                    AttackCommandAssignment mto = new AttackCommandAssignment(player, units.ToList(), target);
                    mto.AssignCommands();
                    return;
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");

        }
    }

    public enum AbilityType
    {
        MOVE_TO,
        ATTACK
    }
}
