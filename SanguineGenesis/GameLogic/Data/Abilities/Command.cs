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
using SanguineGenesis.GUI.WinFormsComponents;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Tells entity what to do.
    /// </summary>
    abstract class Command : IShowable
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
        /// <summary>
        /// Used for logging errors.
        /// </summary>
        public ActionLog ActionLog { get; set; }

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
        /// Returns true if the user animal should keep following the target.
        /// </summary>
        public virtual bool FollowTarget() => false;
        /// <summary>
        /// Command used for following the target.
        /// </summary>
        public MoveToCommand FollowCommand { get; set; }
    }

    abstract class Command<User, TargetT, AbilityT> : Command where User : Entity
                                                                    where TargetT : ITargetable
                                                                    where AbilityT : Ability<User, TargetT>
    {
        /// <summary>
        /// The ability this command is performing.
        /// </summary>
        public AbilityT Ability { get; }
        /// <summary>
        /// The entity who performs this command.
        /// </summary>
        public User CommandedEntity { get; }
        /// <summary>
        /// Target of the ability.
        /// </summary>
        public TargetT Target { get; }
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
                    if (CommandedEntity is Animal a)
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
        public override int Progress 
            => Math.Min(100, Ability.Duration!=0 ? (int)((ElapsedTime / Ability.Duration) * 100) : 0);

        protected Command(User commandedEntity, TargetT target, AbilityT ability)
        {
            Ability = ability;
            CommandedEntity = commandedEntity;
            Target = target;
            ElapsedTime = 0;
            ActionLog = ActionLog.ThrowAway;
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
        /// Refunds energy paid for this ability. Only works if Paid == true;
        /// </summary>
        protected void Refund()
        {
            if (Paid)
                //the ability was paid already
                CommandedEntity.Energy += Ability.EnergyCost;
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
            //check validity of target and user
            if (CommandedEntity.IsDead)
            {
                ActionLog.LogError(CommandedEntity, Ability, "the user is dead");
                return false;
            }
            if (Target is Entity targEnt && targEnt.IsDead)
            {
                return false;
            }

            //check if both target and user are not locked by other ability/status...
            if (CommandedEntity is Animal commandedA)
            {
                if (commandedA.StateChangeLock != null && commandedA.StateChangeLock != this)
                {
                    ActionLog.LogError(CommandedEntity, Ability, "the user is locked by other command/status");
                    return false;
                }
            }
            if (Target is Animal targA)
            {
                if (targA.StateChangeLock != null && targA.StateChangeLock != this)
                {
                    ActionLog.LogError(CommandedEntity, Ability, "the target is locked by other command/status");
                    return false;
                }
            }

            //check if target satisfies additional conditions required by the ability
            if (!Ability.ValidArguments(CommandedEntity, Target, ActionLog))
                return false;

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

            //check CommandedEntity and Targ are close enough
            {
                if (FollowCommand != null)
                {
                    // follow the target animal if user is animal and too far away
                    bool moving = TryFollowTarget(game, deltaT);
                    if (moving)
                        return false;
                }
                else // FollowCommand == null
                {
                    // target is too far away and CommandedEntity can't follow it
                    // => finish ability
                    if (Target.DistanceTo(CommandedEntity) > Distance + .2f)
                    {
                        ActionLog.LogError(CommandedEntity, Ability, "the target is too far away");
                        return true;
                    }
                }
            }

            if (CommandedEntity is Animal a)
                a.CanBeMoved = false;

            ElapsedTime += deltaT;
            bool finished = PerformCommandLogic(game, deltaT);
            if (finished)
            {
                OnRemove();
            }
            return finished;
        }

        /// <summary>
        /// Follow the target animal if user is animal and too far away.
        /// </summary>
        private bool TryFollowTarget(Game game, float deltaT)
        {
            if (FollowTarget() &&
                Target.GetType() != typeof(Nothing) &&
                CommandedEntity is Animal animal)
            {
                float distance = Target.DistanceTo(animal);
                if (distance > Distance)
                {
                    animal.CanBeMoved = true;
                    ElapsedTime = 0;
                    if (!animal.WantsToMove)
                        animal.SetAnimation("RUNNING");
                    FollowCommand.PerformCommand(game, deltaT);
                    return true;
                }
                else
                {
                    animal.CanBeMoved = false;
                    if (animal.WantsToMove)
                    {
                        if (this is AttackCommand)
                            animal.SetAnimation("ATTACKING");
                        else
                            animal.SetAnimation("IDLE");
                        animal.WantsToMove = false;
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

        public override void OnRemove()
        {
            base.OnRemove();
            //animal loses low priority of getting pushed by collisions
            if (CommandedEntity is Animal a)
                a.CanBeMoved = true;
        }

        public override string GetName() => Ability.GetName();
    }

    /// <summary>
    /// Moves animal to point on the map with even movement in given time. During the movement
    /// the animal doesn't check for collisions and can't be used as a target for a command.
    /// </summary>
    class MoveAnimalToPoint
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
        /// Returns true if the animal reached its destination or the MaxWaitingTime was reached.
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