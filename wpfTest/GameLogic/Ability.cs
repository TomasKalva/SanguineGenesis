using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Represents a target on the map.
    /// </summary>
    public interface ITargetable
    {
        Vector2 Center { get; }
    }

    /// <summary>
    /// Place where unit can go to.
    /// </summary>
    public interface IMovementTarget:ITargetable
    {
    }

    public abstract class Ability
    {
        /// <summary>
        /// Maximal distance from the target where the ability can be cast.
        /// </summary>
        public float Distance { get; }
        
        public abstract void SetCommands(IEnumerable<Entity> casters, ITargetable target);
        /*/// <summary>
        /// Assigns commands to the units. All calculations that take long and can be pre-processed
        /// should be put into Process.
        /// </summary>
        public override void SetCommands(Players player, List<Entity> casters, ITargetable target)
        {
            foreach (Entity u in casters)
            {
                Command com = NewCommand(u,target);
                //com.Creator = this;
                u.AddCommand(com);
            }
        }*/
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
        public sealed override Command NewCommand(Entity caster, ITargetable target)
        {
            return NewCommand((Caster)caster, (Target)target);
        }

        public abstract Command NewCommand(Caster caster, Target target);

        /// <summary>
        /// Assigns commands to the units.
        /// </summary>
        public sealed override void SetCommands(IEnumerable<Entity> casters, ITargetable target)
        {
            SetCommands(casters.Cast<Caster>(), (Target)target);
        }

        public virtual void SetCommands(IEnumerable<Caster> casters, Target target)
        {
            //if there are no casters do nothing
            if (!casters.Any())
                return;

            //move to the target until the required distance is reached
            MoveTo.GetMoveTo(this).SetCommands(casters, target);

            //give command to each caster
            foreach (Caster c in casters)
            {
                Command com = NewCommand(c, target);
                c.AddCommand(com);
            }
        }
    }

    public class MoveTo: TargetAbility<Unit,IMovementTarget>,IMovementParametrizing
    {
        private static MoveTo ability;
        /// <summary>
        /// Movement parameters for each ability other than MoveTo.
        /// </summary>
        private static Dictionary<Ability, MoveTo> moveToCast;
        static MoveTo()
        {
            ability = new MoveTo(0.1f, true, false);
            moveToCast = new Dictionary<Ability, MoveTo>();
            moveToCast.Add(Attack.Get, new MoveTo(-1, true, true));
        }
        private MoveTo(float goalDistance, bool interruptable, bool usesAttackDistance)
        {
            GoalDistance = goalDistance;
            Interruptable = interruptable;
            UsesAttackDistance = usesAttackDistance;
        }
        public static MoveTo Get => ability;
        public static MoveTo GetMoveTo(Ability a) => moveToCast[a];

        //interface IMovementParametrizing properties
        public float GoalDistance { get; }
        public bool Interruptable { get; }
        public bool UsesAttackDistance { get; }

        public override void SetCommands(IEnumerable<Unit> casters, IMovementTarget target)
        {
            //if there are no casters do nothing
            if (!casters.Any())
                return;
            //player whose units are receiving commands
            Players player = casters.First().Owner;

            //separete units to different groups by their movement
            var castersGroups = casters.ToLookup((unit) => unit.Movement);

            //volume of all units' circles /pi
            float volume = casters.Select((e) => e.Range * e.Range).Sum();
            //distance from the target when unit can stop if it gets stuck
            float minStoppingDistance = (float)Math.Sqrt(volume) * 1.3f;

            foreach(Movement m in Enum.GetValues(typeof(Movement)))
            {
                IEnumerable<Unit> castersMov = castersGroups[m];
                //set commands only if any unit can receive it
                if (!castersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, casters.Cast<Unit>().ToList(), m, target);
                //give command to each caster and set the command's creator
                foreach (Unit caster in casters)
                {
                    IComputable com = new MoveToPointCommand(caster, target, minStoppingDistance, this);
                    com.Assignment = mtca;

                    caster.AddCommand((Command)com);
                }
                MovementGenerator.GetMovementGenerator().AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Unit caster, IMovementTarget target)
        {
            throw new NotImplementedException("This method is not necessary because the virtual method " + nameof(SetCommands) + " was overriden");
        }
    }

    public sealed class Attack : TargetAbility<Unit, Entity>
    {
        private static Attack ability;
        static Attack()
        {
            ability = new Attack();
        }
        private Attack() { }
        public static Attack Get => ability;
        public override Command NewCommand(Unit caster, Entity target)
        {
            return new AttackCommand(caster, target);
        }
    }
}
