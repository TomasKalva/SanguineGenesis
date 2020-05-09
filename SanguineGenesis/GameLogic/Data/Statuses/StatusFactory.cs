using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Statuses
{
    /// <summary>
    /// Use for creating statuses.
    /// </summary>
    abstract class StatusFactory
    {
        /// <summary>
        /// True iff there can be at most one instance of this status per status owner.
        /// </summary>
        public bool OnlyOnce { get; }
        /// <summary>
        /// Applies the status created by this factory to the status owner. Returns true iff
        /// application was successful.
        /// </summary>
        public abstract bool ApplyToStatusOwner(IStatusOwner affected);

        /// <summary>
        /// Returns type of owner.
        /// </summary>
        public abstract Type OwnerType { get; }

        public abstract string GetName();

        public StatusFactory(bool onlyOnce)
        {
            OnlyOnce = onlyOnce;
        }
    }

    abstract class StatusFactory<Affected>:StatusFactory where Affected : class , IStatusOwner
    {
        public StatusFactory(bool onlyOnce)
            :base(onlyOnce)
        {
        }

        protected abstract Status NewInstance(Affected affected);

        public sealed override bool ApplyToStatusOwner(IStatusOwner affected)
            => ApplyToAffected((Affected)affected);

        /// <summary>
        /// Returns true iff the status was successfuly applied.
        /// </summary>
        public virtual bool ApplyToAffected(Affected affected)
        {
            //if this status can be only applied once, check if it is already applied to the entity
            if (OnlyOnce &&
                affected.Statuses.Where((s) => s.Creator.GetType() == GetType()).Any())
                //status can't be applied second time
                return false;

            //status can be applied
            Status newStatus = NewInstance(affected);
            affected.AddStatus(newStatus);
            return true;
        }

        /// <summary>
        /// Returns type of owner.
        /// </summary>
        public override Type OwnerType => typeof(Affected);

        public override string ToString() => GetName();
    }

    /// <summary>
    /// Marks classes that can own statuses.
    /// </summary>
    interface IStatusOwner
    {
        /// <summary>
        /// Statuses that are affecting this owner.
        /// </summary>
        List<Status> Statuses { get; }
        /// <summary>
        /// Adds status to Statuses.
        /// </summary>
        void AddStatus(Status status);
        /// <summary>
        /// Removes status from Statuses.
        /// </summary>
        void RemoveStatus(Status status);
        /// <summary>
        /// Perfrom step of all statuses.
        /// </summary>
        void StepStatuses(Game game, float deltaT);
    }

    class PoisonFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Damage that the poison does in one tick.
        /// </summary>
        public float TickDamage { get; }
        /// <summary>
        /// Number of ticks of the poison.
        /// </summary>
        public int TotalNumberOfTicks { get; }
        /// <summary>
        /// Length of interval between two consecutive ticks.
        /// </summary>
        public float TickTime { get; }

        public PoisonFactory(float tickDamage, int totalNumberOfTicks, float tickTime)
            : base(false)
        {
            TickDamage = tickDamage;
            TotalNumberOfTicks = totalNumberOfTicks;
            TickTime = tickTime;
        }

        protected override Status NewInstance(Animal affected)
            => new Poison(affected, this);

        public override string GetName() => "POISON";
    }

    class SprintFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Speed gained by the unit on land.
        /// </summary>
        public float SpeedBonus { get; }
        /// <summary>
        /// Energy unit has to pay each second to have this status.
        /// </summary>
        public float EnergyCostPerS { get; }

        public SprintFactory(float speedBonus, float energyPerS)
            : base(true)
        {
            SpeedBonus = speedBonus;
            EnergyCostPerS = energyPerS;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new Sprint(affected, this);
        }

        public override string GetName() => "SPRINT";
    }

    class ConsumedAnimalFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Duration of the unit being consumed.
        /// </summary>
        public float Duration { get; }
        /// <summary>
        /// Animal that is consumed. Should be set right before using this factory to apply status.
        /// </summary>
        public Animal AnimalConsumed { get; set; }

        public ConsumedAnimalFactory(float duration)
            : base(true)
        {
            Duration = duration;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new ConsumedAnimal(affected, this, AnimalConsumed);
        }

        public override string GetName() => "CONSUMED";
    }

    class AnimalsOnPlantFactory : StatusFactory<Plant>
    {
        /// <summary>
        /// Animal that will be put on the tree. Should be set right before using this factory to apply status.
        /// </summary>
        public Animal PutOnTree { get; set; }
        public ClimbDownPlant ClimbDownPlant { get; }

        public AnimalsOnPlantFactory(ClimbDownPlant climbDownPlant)
            : base(true)
        {
            ClimbDownPlant = climbDownPlant;
        }

        protected override Status NewInstance(Plant affected)
        {
            return new AnimalsOnTree(affected, this, PutOnTree);
        }


        public override bool ApplyToAffected(Plant affected)
        {
            AnimalsOnTree alreadyApplied = (AnimalsOnTree)affected.Statuses.Where((s) => s.GetType() == typeof(AnimalsOnTree)).FirstOrDefault();
            if(alreadyApplied!=null)
            {
                //use existing instance of the status
                if (PutOnTree != null)
                {
                    alreadyApplied.Animals.Add(PutOnTree);
                    PutOnTree.Faction.RemoveEntity(PutOnTree);
                    PutOnTree.StateChangeLock = alreadyApplied;
                }
            }
            else
            {
                //create new instance of the status
                AnimalsOnTree newStatus = (AnimalsOnTree)NewInstance(affected);
                affected.AddStatus(newStatus);
            }
            return true;
        }

        public override string GetName() => "ON_TREE";
    }


    class UndergroundFactory : StatusFactory<Structure>
    {
        /// <summary>
        /// Animal that will be put under ground. Should be set right before using this factory to apply status.
        /// </summary>
        public Animal PutUnderground { get; set; }

        public List<Animal> AnimalsUnderGround { get; }

        public UndergroundFactory()
            : base(true)
        {
            AnimalsUnderGround = new List<Animal>();
        }

        protected override Status NewInstance(Structure affected)
        {
            return new Underground(affected, this);
        }

        public override string GetName() => "UNDERGROUND";
    }

    class ShellFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Length of the time interval this staus will be active for in s.
        /// </summary>
        public float Duration { get; }

        public ShellFactory(float duration)
            : base(true)
        {
            Duration = duration;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new Shell(affected, this);
        }

        public override string GetName() => "SHELL";
    }


    class FastStrikesFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Length of the time interval this staus will be active for in s.
        /// </summary>
        public float Duration { get; }
        /// <summary>
        /// Increase of attack speed.
        /// </summary>
        public float AttSpeedIncr { get; }

        public FastStrikesFactory(float duration)
            : base(true)
        {
            Duration = duration;
            AttSpeedIncr = 0.1f;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new FastStrikes(affected, this);
        }

        public override string GetName() => "FAST_STRIKES";
    }

    class FarSightFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// View range increase.
        /// </summary>
        public float RangeExtension { get; }

        public FarSightFactory(float rangeExtension)
            : base(true)
        {
            RangeExtension = rangeExtension;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new FarSight(affected, this);
        }

        public override string GetName() => "FAR_SIGHT";
    }

    class KnockAwayFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// How far the animal is knocked away.
        /// </summary>
        public float Distance { get; }
        /// <summary>
        /// How fast the animal travels.
        /// </summary>
        public float Speed { get; }
        /// <summary>
        /// In which direction the animal is knocked away. Should be unit vector.
        /// Is set before using this factory.
        /// </summary>
        public Vector2 Direction { get; set; }

        public KnockAwayFactory(float distance, float speed)
            : base(true)
        {
            Distance=distance;
            Speed = speed;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new KnockAway(affected, this);
        }

        public override string GetName() => "KNOCK_AWAY";
    }

    class SuffocatingFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// How much damage the animal takes per second.
        /// </summary>
        public float DamagePerS { get; }

        public SuffocatingFactory(float damagePerS)
            : base(true)
        {
            DamagePerS = damagePerS;
        }
        protected override Status NewInstance(Animal affected)
        {
            return new Suffocating(affected, this);
        }

        public override string GetName() => "SUFFOCATING";
    }

    class DecayFactory : StatusFactory<IDecayable>
    {
        /// <summary>
        /// How much energy the corpse/dead plant loses per second.
        /// </summary>
        public float EnergyLossPerS { get; }

        public DecayFactory(float energyLossPerS)
            : base(true)
        {
            EnergyLossPerS = energyLossPerS;
        }
        protected override Status NewInstance(IDecayable affected)
        {
            return new Decay(affected, this);
        }

        public override string GetName() => "DECAY";
    }
}
