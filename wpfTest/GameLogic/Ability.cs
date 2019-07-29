using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public interface ITargetable
    {
        Vector2 Center { get; }
    }

    public abstract class Ability
    {
        public float Distance { get; }
        /// <summary>
        /// True if attack distance of the unit should be used instead of Distance.
        /// </summary>
        public bool UsesAttackDistance { get; }
        
        //public abstract void SetCommands(List<Entity> casters, ITargetable target);
        /// <summary>
        /// Assigns commands to the units. All calculations that take long and can be pre-processed
        /// should be put into Process.
        /// </summary>
        public virtual void SetCommands(Players player, List<Entity> casters, ITargetable target)
        {
            foreach (Entity u in casters)
            {
                Command com = NewCommand(u,target);
                //com.Creator = this;
                u.AddCommand(com);
            }
        }
        public abstract Command NewCommand(Entity caster, ITargetable target);
    }

    public abstract class TargetAbility<Caster, Target> : Ability where Caster:Entity 
                                                                    where Target: ITargetable
    {
        /// <summary>
        /// Calls generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If some casters or target have incompatible type.</exception>
        /// <exception cref="NullReferenceException">If some casters are null.</exception>
        public override Command NewCommand(Entity caster, ITargetable target)
        {
            return NewCommand((Caster)caster, (Target)target);
        }

        public abstract Command NewCommand(Caster caster, Target target);
        
    }

    public class MoveToPoint: TargetAbility<Unit,Vector2>
    {
        private static Ability ability;
        static MoveToPoint()
        {
            ability = new MoveToPoint();
        }
        private MoveToPoint() { }
        public static Ability Get => ability;

        public override void SetCommands(Players player, List<Entity> casters, ITargetable target)
        {
            MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, casters.Cast<Unit>().ToList(), Movement.GROUND, target);
            foreach (Entity u in casters)
            {
                IComputable com = (MoveToPointCommand)NewCommand(u, target);
                com.Creator = mtca;
                
                u.AddCommand((Command)com);
            }
            MovementGenerator.GetMovementGenerator().AddNewCommand(player,mtca);
        }

        public override Command NewCommand(Unit caster, Vector2 target)
        {
            return new MoveToPointCommand(caster, target);
        }
    }
    /*
    public class MoveToUnit: TargetAbility<Unit, Unit>
    {
        private static Ability ability;
        static MoveToUnit()
        {
            ability = new MoveToUnit();
        }
        private MoveToUnit() { }
        public static Ability Get => ability;

        public override Command NewCommand(Unit caster, Unit target)
        {
            return new MoveToUnitCommand(caster, target);
        }
    }*/

    public sealed class Attack : TargetAbility<Unit, Entity>
    {
        private static Ability ability;
        static Attack()
        {
            ability = new Attack();
        }
        private Attack() { }
        public static Ability Get => ability;
        public override Command NewCommand(Unit caster, Entity target)
        {
            return new AttackCommand(caster, target);
        }
    }
    /*
    public class TargetPointAbility:Ability
    {
        public TargetPointAbility(AbilityType abilityType, float distance, bool usesAttackDistance=false)
            :base(abilityType,distance, usesAttackDistance)
        { }

        /// <summary>
        /// Assigns a new instance of the command assignment for this ability and returns it.
        /// </summary>
        public void AssignCommands(Players player, List<Entity> units, Vector2 target, Game game)
        {
            //move to the target until the minimal distance is reached
            if(!(AbilityType==AbilityType.MOVE_TO))
                AssignMovementToPoint(player, units, target, game);

            switch (AbilityType)
            {
                case AbilityType.MOVE_TO:
                    {
                        //create new instace of move to command assignment and set it to all units
                        MoveToCommandAssignment mto = new MoveToPointCommandAssignment(player, units.ToList(), target, Movement.GROUND, Distance,true);
                        mto.AssignCommands();
                        //add the command assignment to movement generator to create the flowmap asynchronously
                        MovementGenerator.GetMovementGenerator().AddNewCommand(Players.PLAYER0,mto);
                        return;
                    }
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");
        }

        /// <summary>
        /// Assigns commands to units to move to the target until they are at most the goalDistance
        /// from it
        /// </summary>
        /// <param name="endDistance">Distance to the target when the unit stops moving.</param>
        protected void AssignMovementToPoint(Players player, List<Entity> units, Vector2 target, Game game)
        {
            //create new instace of move to command assignment and set it to all units
            MoveToCommandAssignment mto = new MoveToPointCommandAssignment(player, units.ToList(), target, Movement.GROUND, Distance);
            mto.AssignCommands();
            //add the command assignment to movement generator to create the flowmap asynchronously
            MovementGenerator.GetMovementGenerator().AddNewCommand(Players.PLAYER0, mto);
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
        public void AssignCommands(Players player, List<Entity> units, Entity target, Game game)
        {
            //move to the target until the minimal distance is reached
            if (!(AbilityType == AbilityType.MOVE_TO))
                AssignMovementToUnit(player, units, target, game);

            switch (AbilityType)
            {
                case AbilityType.ATTACK:
                    AttackCommandAssignment mto = new AttackCommandAssignment(player, units.ToList(), target);
                    mto.AssignCommands();
                    return;
            }
            throw new NotImplementedException("Implementation for " + AbilityType + " in the method " + nameof(AssignCommands) + "is missing!");
        }

        /// <summary>
        /// Assigns commands to units to move to the target unit until they are at most the Distance required
        /// by this ability from it.
        /// </summary>
        protected void AssignMovementToUnit(Players player, List<Entity> units, Entity target, Game game)
        {
            //create new instace of move to command assignment and set it to all units
            MoveToCommandAssignment mto = new MoveToUnitCommandAssignment(player, units.ToList(), target, Movement.GROUND, Distance, UsesAttackDistance);
            mto.AssignCommands();
            //add the command assignment to movement generator to create the flowmap asynchronously
            MovementGenerator.GetMovementGenerator().AddNewCommand(Players.PLAYER0, mto);
        }
    }*/
}
