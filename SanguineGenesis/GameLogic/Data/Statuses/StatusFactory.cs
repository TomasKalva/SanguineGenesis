using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Use for creating statuses.
    /// </summary>
    abstract class StatusFactory
    {
        /// <summary>
        /// True iff there can be at most one instance of this status per entity.
        /// </summary>
        public bool OnlyOnce { get; }
        /// <summary>
        /// Applies the status created by this factory to the entity. Returns true iff
        /// application was successful.
        /// </summary>
        public abstract bool ApplyToEntity(Entity affectedEntity);

        public StatusFactory(bool onlyOnce)
        {
            OnlyOnce = onlyOnce;
        }
    }

    abstract class StatusFactory<Affected>:StatusFactory where Affected:Entity
    {
        public StatusFactory(bool onlyOnce)
            :base(onlyOnce)
        {
        }

        protected abstract Status NewInstance(Affected affectedEntity);

        public sealed override bool ApplyToEntity(Entity affectedEntity)
            => ApplyToAffected((Affected)affectedEntity);

        public virtual bool ApplyToAffected(Affected affectedEntity)
        {
            Status newStatus = NewInstance(affectedEntity);

            //if this status can be only applied once, check if it is already applied to the entity
            if (OnlyOnce &&
                affectedEntity.Statuses.Where((s) => s.GetType() == newStatus.GetType()).Any())
                //status can't be applied second time
                return false;

            //status can be applied
            affectedEntity.AddStatus(newStatus);
            return true;
        }

        public override string ToString() => NewInstance(null).GetName();
    }

    class PoisonFactory : StatusFactory<Entity>
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

        protected override Status NewInstance(Entity affectedEntity)
            => new Poison(affectedEntity, this);
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
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new Sprint(affectedEntity, this);
        }
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
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new ConsumedAnimal(affectedEntity, this, AnimalConsumed);
        }
    }

    class AnimalsOnTreeFactory : StatusFactory<Tree>
    {
        /// <summary>
        /// Animal that will be put on the tree. Should be set right before using this factory to apply status.
        /// </summary>
        public Animal PutOnTree { get; set; }

        public AnimalsOnTreeFactory()
            : base(true)
        {
        }
        protected override Status NewInstance(Tree affectedEntity)
        {
            return new AnimalsOnTree(affectedEntity, this, PutOnTree);
        }


        public override bool ApplyToAffected(Tree affectedEntity)
        {
            AnimalsOnTree alreadyApplied = (AnimalsOnTree)affectedEntity.Statuses.Where((s) => s.GetType() == typeof(AnimalsOnTree)).FirstOrDefault();
            if(alreadyApplied!=null)
            {
                if (PutOnTree != null)
                {
                    alreadyApplied.Animals.Add(PutOnTree);
                    PutOnTree.Faction.Entities.Remove(PutOnTree);
                    PutOnTree.StateChangeLock = alreadyApplied;
                }
            }
            else
            {
                AnimalsOnTree newStatus = (AnimalsOnTree)NewInstance(affectedEntity);
                affectedEntity.AddStatus(newStatus);
            }
            return true;
        }
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

        protected override Status NewInstance(Structure affectedEntity)
        {
            return new Underground(affectedEntity, this);
        }
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
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new Shell(affectedEntity, this);
        }
    }


    class FastStrikesFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Length of the time interval this staus will be active for in s.
        /// </summary>
        public float Duration { get; }

        public FastStrikesFactory(float duration)
            : base(true)
        {
            Duration = duration;
        }
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new FastStrikes(affectedEntity, this);
        }
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
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new FarSight(affectedEntity, this);
        }
    }

    class KnockBackFactory : StatusFactory<Animal>
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

        public KnockBackFactory(float distance, float speed)
            : base(true)
        {
            Distance=distance;
            Speed = speed;
        }
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new KnockAway(affectedEntity, this);
        }
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
        protected override Status NewInstance(Animal affectedEntity)
        {
            return new Suffocating(affectedEntity, this);
        }
    }
}
