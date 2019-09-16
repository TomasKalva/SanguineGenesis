using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GUI;
using static SanguineGenesis.MainWindow;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Can be applied to Entity. Does something.
    /// </summary>
    public abstract class Status: IShowable
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

        #region IShowable
        public abstract string GetName();
        public virtual List<Stat> Stats()
        {
           return new List<Stat>();
        }
        public abstract string Description();
        #endregion IShowable
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

    /// <summary>
    /// Increases speed.
    /// </summary>
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
            //remove the status if the animal doesn't have enough energy
            AffectedEntity.Energy -= StatusInfo.EnergyCostPerS * (decimal)deltaT;
            if (AffectedEntity.Energy <= 0)
                return true;
            return false;
        }

        public override string GetName() => "Sprint";

        public override string Description()
        {
            return "Increases speed of animal by " + StatusInfo.SpeedBonus + ".";
        }
    }

    /// <summary>
    /// Represents consumed animal by this animal.
    /// </summary>
    public class ConsumedAnimal : Status<Animal, ConsumedAnimalFactory>, IAnimalStateManipulator
    {
        /// <summary>
        /// Animal that is consumed.
        /// </summary>
        public Animal AnimalConsumed { get; }
        /// <summary>
        /// Time the animal has been consumed in s.
        /// </summary>
        private float timeElapsed;

        public ConsumedAnimal(Animal affectedEntity, ConsumedAnimalFactory consumeInfo, Animal animalConsumed)
            : base(affectedEntity, consumeInfo)
        {
            AnimalConsumed = animalConsumed;
            AnimalConsumed.StateChangeLock = this;
            timeElapsed = 0;
        }

        public override void Added()
        {
            //temporarily remove the animal from list of entities
            AnimalConsumed.Player.Entities.Remove(AnimalConsumed);
            AnimalConsumed.StateChangeLock = this;
        }

        public override void Removed()
        {
            //add the consumed unit back to the list of entities
            //it spawns in front of the affected animal
            AnimalConsumed.Position = AffectedEntity.Position + (AffectedEntity.Range + AnimalConsumed.Range) * AffectedEntity.Direction;
            AnimalConsumed.Player.Entities.Add(AnimalConsumed);
            AnimalConsumed.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the command if the whole duration elapsed
            timeElapsed += deltaT;
            if (timeElapsed>=StatusInfo.Duration)
                return true;
            return false;
        }

        public override string GetName() => "Consumed";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return "Consumed another animal for " + StatusInfo.Duration + " seconds.";
        }
    }

    /// <summary>
    /// Deals damage over time.
    /// </summary>
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

        public override string GetName() => "Poison";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Damage", StatusInfo.TickDamage.ToString()),
                new Stat( "Tick time", StatusInfo.TickTime.ToString()),
                new Stat( "Ticks", StatusInfo.TotalNumberOfTicks.ToString()),
            };
        }

        public override string Description()
        {
            return "Deals " + StatusInfo.TickDamage + " damage every "+StatusInfo.TickTime+" seconds.";
        }
    }

    /// <summary>
    /// Gives thick skin.
    /// </summary>
    public class Shell : Status<Animal, ShellFactory>
    {
        private float timer;

        public Shell(Animal affectedEntity, ShellFactory shellInfo)
            : base(affectedEntity, shellInfo)
        {
            timer = 0f;
        }

        public override void Added()
        {
            AffectedEntity.ThickSkin = true;
        }

        public override void Removed()
        {
            AffectedEntity.ThickSkin = false;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusInfo.Duration)
                return true;
            return false;
        }

        public override string GetName() => "Shell";

        public override string Description()
        {
            return "Gives animal thick skin.";
        }
    }

    /// <summary>
    /// Increases attack speed.
    /// </summary>
    public class FastStrikes : Status<Animal, FastStrikesFactory>
    {
        private float timer;

        public FastStrikes(Animal affectedEntity, FastStrikesFactory fastStrikesInfo)
            : base(affectedEntity, fastStrikesInfo)
        {
            timer = 0f;
        }

        public override void Added()
        {
            AffectedEntity.AttackPeriod -= 0.1f;
        }

        public override void Removed()
        {
            AffectedEntity.AttackPeriod += 0.1f;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusInfo.Duration)
                return true;
            return false;
        }

        public override string GetName() => "Fast strikes";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return "Increases attack speed of animal by " + 0.1f + ".";
        }
    }

    /// <summary>
    /// Represents animal on tree.
    /// </summary>
    public class AnimalsOnTree : Status<Tree, AnimalsOnTreeFactory>, IAnimalStateManipulator
    {
        public List<Animal> Animals { get; }

        public AnimalsOnTree(Tree affectedEntity, AnimalsOnTreeFactory animalsOnTreenfo, Animal putOnTree)
            : base(affectedEntity, animalsOnTreenfo)
        {
            Animals = new List<Animal>(1) { putOnTree };
        }

        public override void Added()
        {
            //add ability for animals climbing down from the tree
            AffectedEntity.Abilities.Add(AffectedEntity.Player.GameStaticData.Abilities.ClimbDownTree);
        }

        public override void Removed()
        {
            //remove ability for animals climbing down from the tree
            AffectedEntity.Abilities.Remove(AffectedEntity.Player.GameStaticData.Abilities.ClimbDownTree);
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override string GetName() => "On tree";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", Animals.Count.ToString())
            };
        }

        public override string Description()
        {
            return "There are " + Animals.Count() + " animals on this tree.";
        }
    }

    /// <summary>
    /// Represents animals underground.
    /// </summary>
    public class Underground : Status<Structure, UndergroundFactory>, IAnimalStateManipulator
    {
        public List<Animal> AnimalsUnderGround => StatusInfo.AnimalsUnderGround;

        public Underground(Structure affectedEntity, UndergroundFactory undergroundInfo)
            : base(affectedEntity, undergroundInfo)
        {
        }

        public override void Added()
        {
            //do nothing
        }

        public override void Removed()
        {
            //do nothing
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override string GetName() => "Underground";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", AnimalsUnderGround.Count.ToString())
            };
        }

        public override string Description()
        {
            return "There are " + AnimalsUnderGround.Count() + " underground.";
        }
    }

    /// <summary>
    /// Increases view range.
    /// </summary>
    public class FarSight : Status<Animal, FarSightFactory>
    {
        public FarSight(Animal affectedEntity, FarSightFactory farSightInfo)
            : base(affectedEntity, farSightInfo)
        {
        }

        public override void Added()
        {
            AffectedEntity.ViewRange += StatusInfo.RangeExtension;
        }

        public override void Removed()
        {
            AffectedEntity.ViewRange -= StatusInfo.RangeExtension;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the unit moves
            if (AffectedEntity.WantsToMove)
                return true;
            return false;
        }

        public override string GetName() => "Far sight";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Range bonus", StatusInfo.RangeExtension.ToString())
            };
        }

        public override string Description()
        {
            return "Increases view range by " + StatusInfo.RangeExtension + ".";
        }
    }

    /// <summary>
    /// Animal is being knocked away.
    /// </summary>
    public class KnockAway : Status<Animal, KnockAwayFactory>, IAnimalStateManipulator
    {
        private MoveAnimalToPoint moveAnimalToPoint;

        public KnockAway(Animal affectedEntity, KnockAwayFactory knockBackInfo)
            : base(affectedEntity, knockBackInfo)
        {
            Vector2 targetPoint = affectedEntity.Position + StatusInfo.Distance * StatusInfo.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(affectedEntity, targetPoint, StatusInfo.Speed);
        }

        public override void Added()
        {
            AffectedEntity.StateChangeLock = this;
        }

        public override void Removed()
        {
            AffectedEntity.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            return moveAnimalToPoint.Step(deltaT);
        }

        public override string GetName() => "Knocked away";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Distance", StatusInfo.Distance.ToString())
            };
        }

        public override string Description()
        {
            return "This animal is being knocked away.";
        }
    }
}
