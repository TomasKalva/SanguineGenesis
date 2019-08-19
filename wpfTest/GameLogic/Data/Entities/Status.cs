using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Entities
{
    public abstract class Status
    {
        /// <summary>
        /// Called when the status is added to the entity.
        /// </summary>
        public abstract void Added();
        /// <summary>
        /// Called on game update. Returns true if the status was finished and should be removed.
        /// </summary>
        public abstract bool Step(Game game, float deltaT);
        /// <summary>
        /// Called when the status is removed from the entity.
        /// </summary>
        public abstract void Removed();
    }

    public abstract class Status<Affected,Info>:Status where Info:StatusFactory<Affected>
                                                        where Affected:Entity
    {
        /// <summary>
        /// Entity affected by this status.
        /// </summary>
        public Affected AffectedEntity { get; }
        /// <summary>
        /// Immutable information about the status.
        /// </summary>
        protected Info StatusInfo { get; }

        public Status(Affected affectedEntity, Info statusInfo)
        {
            AffectedEntity = affectedEntity;
            StatusInfo = statusInfo;
        }
    }

    public class Sprint : Status<Animal, SprintFactory>
    {
        public Sprint(Animal affectedEntity, SprintFactory sprintInfo)
            :base(affectedEntity, sprintInfo)
        {
        }

        public override void Added()
        {
            AffectedEntity.MaxSpeedLand += StatusInfo.SpeedBonus;
        }

        public override void Removed()
        {
            AffectedEntity.MaxSpeedLand -= StatusInfo.SpeedBonus;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the command if the unit doesn't have enough energy
            AffectedEntity.Energy -= StatusInfo.EnergyCostPerS * (decimal)deltaT;
            if (AffectedEntity.Energy <= 0)
                return true;
            return false;
        }
    }

    public class Poison : Status<Entity, PoisonFactory>
    {
        /// <summary>
        /// Time from the last tick in s.
        /// </summary>
        private float tickTimeElapsed;
        /// <summary>
        /// Number of ticks already performed.
        /// </summary>
        private int ticksPerformed;


        public Poison(Entity affectedEntity, PoisonFactory poisonInfo)
            : base(affectedEntity, poisonInfo)
        {
            ticksPerformed = 0;
            tickTimeElapsed = 0f;
        }

        public override void Added()
        {
            //nothing to do
        }

        public override void Removed()
        {
            //nothing to do
        }

        public override bool Step(Game game, float deltaT)
        {
            tickTimeElapsed += deltaT;
            if (tickTimeElapsed > StatusInfo.TickTime)
            {
                //reset timer
                tickTimeElapsed -= StatusInfo.TickTime;
                //deal damage to the entity and increment number of performed ticks
                AffectedEntity.Damage(StatusInfo.TickDamage);
                ticksPerformed++;
            }

            //finish if all ticks have been performed
            return ticksPerformed > StatusInfo.TotalNumberOfTicks;
        }
    }
}
