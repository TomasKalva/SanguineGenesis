using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Abilities;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public abstract class Command
    {
        /// <summary>
        /// True iff the command can be removed from the first place in the command queue.
        /// </summary>
        public abstract bool Interruptable { get; }

        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand(Game game, float deltaT);
    }

    public abstract class Command<Caster, Target, Abil> : Command where Caster : Entity
                                                                    where Target : ITargetable
                                                                    where Abil:TargetAbility<Caster,Target>
    {
        /// <summary>
        /// The ability this command is performing.
        /// </summary>
        public Abil Ability { get; }
        /// <summary>
        /// The entity who performs this command.
        /// </summary>
        public Caster CommandedEntity { get; }
        /// <summary>
        /// Target of the ability.
        /// </summary>
        public Target Targ { get; }
        /// <summary>
        /// True iff the ability was paid.
        /// </summary>
        private bool Paid { get; set; }
        public override bool Interruptable => Ability.Interruptable;

        protected Command(Caster commandedEntity, Target target, Abil ability)
        {
            Ability = ability;
            CommandedEntity = commandedEntity;
            Targ = target;
        }
        protected Command() { }
        public override string ToString()
        {
            return Ability.ToString();
        }

        /// <summary>
        /// Try to pay for the ability. Returns if paying was successful.
        /// </summary>
        protected bool TryPay()
        {
            if (Paid)
                //the ability was paid already
                return true;

            if (CommandedEntity.Energy >= Ability.EnergyCost)
            {
                //pay for the ability
                CommandedEntity.Energy -= Ability.EnergyCost;
                Paid = true;
                return true;
            }
            //not enough energy
            return false;
        }

        /// <summary>
        /// Returns if entity can pay this ability.
        /// </summary>
        protected bool CanPay()
        {
            if (Paid)
                //the ability was paid already
                return true;
            else
                return CommandedEntity.Energy >= Ability.EnergyCost;
        }
        
        /// <summary>
        /// Returns true iff the commanded entity and target can be used by this command.
        /// </summary>
        protected bool CanBeUsed()
        {
            if (CommandedEntity.IsDead)
                return false;
            Entity targEnt = Targ as Entity;
            if (targEnt!=null && targEnt.IsDead)
                return false;

            Animal commandedA = CommandedEntity as Animal;
            if (commandedA != null)
            {
                if (commandedA.StateChangeLock != null && commandedA.StateChangeLock != this)
                    return false;
            }
            Animal targA = Targ as Animal;
            if (targA != null)
            {
                if (targA.StateChangeLock != null && targA.StateChangeLock != this)
                    return false;
            }
            return true;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!CanBeUsed())
                //finish if the target or commanded entity is invalid
                return true;

            if (!TryPay())
                //finish command if paying was unsuccessful
                return true;

            return PerformCommandLogic(game, deltaT);
        }

        public abstract bool PerformCommandLogic(Game game, float deltaT);
    }

    /// <summary>
    /// Moves animal to point on the map with even movement in given time. During the movement
    /// the animal doesn't check for collisions and can't be used as a target for a command.
    /// </summary>
    public class MoveAnimalToPoint
    {
        public Animal Animal { get; }
        public IMovementTarget Target { get; }
        public float MaxWaitTime { get; }
        public float Speed { get; }
        private float timer;

        public MoveAnimalToPoint(Animal animal, IMovementTarget point, float speed)
        {
            Animal = animal;
            Target = point;
            Speed = speed;
            MaxWaitTime = Target.DistanceTo(Animal) / speed;
            timer = 0f;
        }

        /// <summary>
        /// Returns true if the unit reached its destination.
        /// </summary>
        public bool Step(float deltaT)
        {
            //animal can jump over all obstacles
            Animal.Physical = false;
            //Animal.CanBeTarget = false;
            //the animal can't move naturally
            Animal.Velocity = new Vector2(0, 0);
            //calculate maximal displacement of Animal
            float posChange = deltaT * Speed;
            float distanceToPoint = Target.DistanceTo(Animal);
            float speed = Math.Min(posChange, distanceToPoint);
            //change Animal's position
            Animal.Position += speed * Animal.Position.UnitDirectionTo(Target.Center);

            timer += deltaT;
            //finish command if Animal is close enough or the time is over
            if (timer >= MaxWaitTime || posChange >= distanceToPoint)
            {
                Animal.Physical = true;
                //Animal.CanBeTarget = true;
                return true;
            }

            return false;
        }
    }
}