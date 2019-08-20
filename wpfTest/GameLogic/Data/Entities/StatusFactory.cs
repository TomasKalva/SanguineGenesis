using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Entities
{
    public abstract class StatusFactory
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

    public abstract class StatusFactory<Affected>:StatusFactory where Affected:Entity
    {
        public StatusFactory(bool onlyOnce)
            :base(onlyOnce)
        {
        }

        protected abstract Status NewInstance(Affected affectedEntity);

        public sealed override bool ApplyToEntity(Entity affectedEntity)
            => ApplyToAffected((Affected)affectedEntity);

        public bool ApplyToAffected(Affected affectedEntity)
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
    }

    public class PoisonFactory : StatusFactory<Entity>
    {
        /// <summary>
        /// Damage that the poison does in one tick.
        /// </summary>
        public decimal TickDamage { get; }
        /// <summary>
        /// Number of ticks of the poison.
        /// </summary>
        public int TotalNumberOfTicks { get; }
        /// <summary>
        /// Length of interval between two consecutive ticks.
        /// </summary>
        public float TickTime { get; }

        public PoisonFactory(decimal tickDamage, int totalNumberOfTicks, float tickTime)
            : base(false)
        {
            TickDamage = tickDamage;
            TotalNumberOfTicks = totalNumberOfTicks;
            TickTime = tickTime;
        }

        protected override Status NewInstance(Entity affectedEntity)
            => new Poison(affectedEntity, this);
    }

    public class SprintFactory : StatusFactory<Animal>
    {
        /// <summary>
        /// Speed gained by the unit on land.
        /// </summary>
        public float SpeedBonus { get; }
        /// <summary>
        /// Energy unit has to pay each second to have this status.
        /// </summary>
        public decimal EnergyCostPerS { get; }

        public SprintFactory(float speedBonus, decimal energyPerS)
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

    public class ConsumedAnimalFactory : StatusFactory<Animal>
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

    public class ShellFactory : StatusFactory<Animal>
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

    public class FarSightFactory : StatusFactory<Animal>
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
}
