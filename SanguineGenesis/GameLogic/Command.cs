using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis
{
    /// <summary>
    /// Tells entity what to do.
    /// </summary>
    public abstract class Command : IShowable
    {
        /// <summary>
        /// True iff the command can be removed from the first place in the command queue.
        /// </summary>
        public abstract bool Interruptable { get; }
        /// <summary>
        /// How much of the command is done. From the interval [0,100]. Used only for player's information.
        /// </summary>
        public abstract int Progress { get; }

        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand(Game game, float deltaT);

        #region IShowable
        public abstract string GetName();
        public List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            { };
            return stats;
        }
        public abstract string Description();
        #endregion IShowable

        /// <summary>
        /// Removes the command from the entity if possible.
        /// </summary>
        public abstract void Remove();

        /// <summary>
        /// Called when this command is removed from command queue.
        /// </summary>
        public virtual void OnRemove()
        {
            if (FollowCommand != null)
                FollowCommand.OnRemove();
        }

        /// <summary>
        /// Returns true if the caster animal should keep following the target.
        /// </summary>
        public virtual bool FollowTarget() => false;
        /// <summary>
        /// Command used for following the target.
        /// </summary>
        public MoveToCommand FollowCommand { get; set; }
    }

    public abstract class Command<Caster, Target, Abil> : Command where Caster : Entity
                                                                    where Target : ITargetable
                                                                    where Abil : TargetAbility<Caster, Target>
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
        /// Required distance between the animal and the target.
        /// </summary>
        public float Distance
        {
            get
            {
                if (Ability.Distance != null)
                    return Ability.Distance.Value;
                else
                {
                    Animal a = CommandedEntity as Animal;
                    if (a != null)
                        return a.AttackDistance;
                    else
                        return 0;
                }
            }
        }
        /// <summary>
        /// True iff the ability was paid.
        /// </summary>
        protected bool Paid { get; private set; }
        /// <summary>
        /// Time elapsed since start of this command.
        /// </summary>
        protected float ElapsedTime { get; set; }
        /// <summary>
        /// True iff it can be removed while being performed.
        /// </summary>
        public override bool Interruptable => Ability.Interruptable;
        /// <summary>
        /// How much of the command is done. From the interval [0,100]. Used only for player's information.
        /// </summary>
        public override int Progress => (int)((ElapsedTime / Ability.Duration) * 100);

        protected Command(Caster commandedEntity, Target target, Abil ability)
        {
            Ability = ability;
            CommandedEntity = commandedEntity;
            Targ = target;
            ElapsedTime = 0;
        }

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

        /// <summary>
        /// If the command can be used and entity is able to pay for it or 
        /// already paid for it, call the PerformCommandLogic.
        /// </summary>
        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!CanBeUsed())
                //finish if the target or commanded entity is invalid
                return true;

            if (!TryPay())
                //finish command if paying was unsuccessful
                return true;

            //follow the target animal if caster is animal and too far away
            bool moving = TryFollowTarget(game, deltaT);
            if (moving)
                return false;

            ElapsedTime += deltaT;
            bool finished = PerformCommandLogic(game, deltaT);
            if (finished)
            {
                OnRemove();
            }
            return finished;
        }

        /// <summary>
        /// Follow the target animal if caster is animal and too far away.
        /// </summary>
        private bool TryFollowTarget(Game game, float deltaT)
        {
            Animal animal = CommandedEntity as Animal;
            if (FollowCommand != null &&
                FollowTarget() &&
                Targ.GetType() != typeof(Nothing) &&
                animal != null)
            {
                float distance = ((IMovementTarget)Targ).DistanceTo(animal);
                if (distance > Distance)
                {
                    ElapsedTime = 0;
                    if (!animal.WantsToMove)
                        animal.SetAnimation("RUNNING");
                    FollowCommand.PerformCommandLogic(game, deltaT);
                    return true;
                }
                else
                {
                    if (animal.WantsToMove)
                    {
                        if (this is AttackCommand)
                            animal.SetAnimation("ATTACKING");
                        else
                            animal.SetAnimation("IDLE");
                        animal.StopMoving = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommandLogic(Game game, float deltaT);

        public override string Description()
        {
            return Ability.Description();
        }

        /// <summary>
        /// Remove this command from the commanded entity if possible.
        /// </summary>
        public override void Remove()
        {
            CommandedEntity.RemoveCommand(this);
        }

        public override string GetName() => Ability.GetName();
    }

    /// <summary>
    /// Moves animal to point on the map with even movement in given time. During the movement
    /// the animal doesn't check for collisions and can't be used as a target for a command.
    /// </summary>
    public class MoveAnimalToPoint
    {
        public Animal Animal { get; }
        public IMovementTarget Target { get; }
        /// <summary>
        /// Maximal waiting time until the moving finishes.
        /// </summary>
        public float MaxWaitTime { get; }
        public float Speed { get; }
        private float timer;

        public MoveAnimalToPoint(Animal animal, IMovementTarget point, float speed, float maxWaitTime)
        {
            Animal = animal;
            Target = point;
            Speed = speed;
            MaxWaitTime = maxWaitTime;
            timer = 0f;
        }

        /// <summary>
        /// Returns true if the unit reached its destination or the MaxWaitingTime was reached.
        /// </summary>
        public bool Step(float deltaT)
        {
            //animal can jump over all obstacles
            Animal.Physical = false;
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
                return true;
            }

            return false;
        }
    }
}